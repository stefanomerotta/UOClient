using System;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace UOClient.Utilities
{
    // https://github.com/Thealexbarney/LibHac/blob/master/src/LibHac/FsSystem/ValueStringBuilder.cs
    internal ref struct ValueStringBuilder
    {
        private char[]? arrayToReturnToPool;
        private int pos;

        /// <summary>Returns the underlying storage of the builder.</summary>
        public Span<char> RawChars { get; private set; }

        public int Length
        {
            get => pos;
            set
            {
                Debug.Assert(value >= 0);
                Debug.Assert(value <= RawChars.Length);
                pos = value;
            }
        }

        public int Capacity => RawChars.Length;

        public ref char this[int index]
        {
            get
            {
                Debug.Assert(index < pos);
                return ref RawChars[index];
            }
        }

        // If this ctor is used, you cannot pass in stackalloc ROS for append/replace.
        public ValueStringBuilder(ReadOnlySpan<char> initialString)
            : this(initialString.Length)
        {
            Append(initialString);
        }

        public ValueStringBuilder(ReadOnlySpan<char> initialString, Span<char> initialBuffer)
            : this(initialBuffer)
        {
            Append(initialString);
        }

        public ValueStringBuilder(Span<char> initialBuffer)
        {
            arrayToReturnToPool = null;
            RawChars = initialBuffer;
            pos = 0;
        }

        public ValueStringBuilder(int initialCapacity)
        {
            arrayToReturnToPool = ArrayPool<char>.Shared.Rent(initialCapacity);
            RawChars = arrayToReturnToPool;
            pos = 0;
        }

        public void EnsureCapacity(int capacity)
        {
            if (capacity > RawChars.Length)
                Grow(capacity - RawChars.Length);
        }

        /// <summary>
        /// Get a pinnable reference to the builder.
        /// Does not ensure there is a null char after <see cref="Length"/>
        /// This overload is pattern matched in the C# 7.3+ compiler so you can omit
        /// the explicit method call, and write eg "fixed (char* c = builder)"
        /// </summary>
        public ref char GetPinnableReference()
        {
            return ref MemoryMarshal.GetReference(RawChars);
        }

        /// <summary>
        /// Get a pinnable reference to the builder.
        /// </summary>
        /// <param name="terminate">Ensures that the builder has a null char after <see cref="Length"/></param>
        public ref char GetPinnableReference(bool terminate)
        {
            if (terminate)
            {
                EnsureCapacity(Length + 1);
                RawChars[Length] = '\0';
            }
            return ref MemoryMarshal.GetReference(RawChars);
        }

        public override string ToString()
        {
            string s = RawChars[..pos].ToString();
            Dispose();
            return s;
        }

        /// <summary>
        /// Returns a span around the contents of the builder.
        /// </summary>
        /// <param name="terminate">Ensures that the builder has a null char after <see cref="Length"/></param>
        public ReadOnlySpan<char> AsSpan(bool terminate)
        {
            if (terminate)
            {
                EnsureCapacity(Length + 1);
                RawChars[Length] = '\0';
            }

            return RawChars[..pos];
        }

        public ReadOnlySpan<char> AsSpan() => RawChars[..pos];
        public ReadOnlySpan<char> AsSpan(int start) => RawChars[start..pos];
        public ReadOnlySpan<char> AsSpan(int start, int length) => RawChars.Slice(start, length);

        public bool TryCopyTo(Span<char> destination, out int charsWritten)
        {
            if (RawChars[..pos].TryCopyTo(destination))
            {
                charsWritten = pos;
                Dispose();
                return true;
            }
            else
            {
                charsWritten = 0;
                Dispose();
                return false;
            }
        }

        public void Insert(int index, char value, int count)
        {
            if (pos > RawChars.Length - count)
                Grow(count);

            int remaining = pos - index;
            RawChars.Slice(index, remaining).CopyTo(RawChars[(index + count)..]);
            RawChars.Slice(index, count).Fill(value);
            pos += count;
        }

        public void Insert(int index, ReadOnlySpan<char> s)
        {
            int count = s.Length;

            if (pos > (RawChars.Length - count))
                Grow(count);

            int remaining = pos - index;
            RawChars.Slice(index, remaining).CopyTo(RawChars[(index + count)..]);
            s.CopyTo(RawChars[index..]);
            pos += count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Append(char c)
        {
            int pos = this.pos;
            if ((uint)pos < (uint)RawChars.Length)
            {
                RawChars[pos] = c;
                this.pos = pos + 1;
            }
            else
            {
                GrowAndAppend(c);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Append(string s)
        {
            int pos = this.pos;
            if (s.Length == 1 && (uint)pos < (uint)RawChars.Length) // very common case, e.g. appending strings from NumberFormatInfo like separators, percent symbols, etc.
            {
                RawChars[pos] = s[0];
                this.pos = pos + 1;
            }
            else
            {
                AppendSlow(s);
            }
        }

        private void AppendSlow(string s)
        {
            int pos = this.pos;
            if (pos > RawChars.Length - s.Length)
                Grow(s.Length);

            s.AsSpan().CopyTo(RawChars[pos..]);
            this.pos += s.Length;
        }

        public void Append(char c, int count)
        {
            if (pos > RawChars.Length - count)
                Grow(count);

            Span<char> dst = RawChars.Slice(pos, count);
            for (int i = 0; i < dst.Length; i++)
            {
                dst[i] = c;
            }
            pos += count;
        }

        public void Append(ReadOnlySpan<char> value)
        {
            int pos = this.pos;
            if (pos > RawChars.Length - value.Length)
                Grow(value.Length);

            value.CopyTo(RawChars[this.pos..]);
            this.pos += value.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<char> AppendSpan(int length)
        {
            int origPos = pos;
            if (origPos > RawChars.Length - length)
                Grow(length);

            pos = origPos + length;
            return RawChars.Slice(origPos, length);
        }

        public void Replace(ReadOnlySpan<char> oldChars, ReadOnlySpan<char> newChars)
        {
            Replace(oldChars, newChars, 0, pos);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Replace(ReadOnlySpan<char> oldChars, ReadOnlySpan<char> newChars, int startIndex, int count)
        {
            Span<char> slice = RawChars.Slice(startIndex, count);
            int indexOf = slice.IndexOf(oldChars);

            if (indexOf == -1)
                return;

            if (newChars.Length > oldChars.Length)
            {
                int i = 0;

                for (; i < oldChars.Length; ++i)
                {
                    slice[indexOf + i] = newChars[i];
                }

                Insert(indexOf + i, newChars[i..]);
            }
            else if (newChars.Length < oldChars.Length)
            {
                int i = 0;

                for (; i < newChars.Length; ++i)
                {
                    slice[indexOf + i] = newChars[i];
                }

                Remove(indexOf + i, oldChars.Length - i);
            }
            else
            {
                newChars.CopyTo(slice[..oldChars.Length]);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Replace(char oldChar, char newChar)
        {
            Span<char> slice = RawChars;
            int indexOf = slice.IndexOf(oldChar);

            if (indexOf == -1)
                return;

            slice[indexOf] = newChar;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(int startIndex, int length)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length));

            if (startIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            if (length > pos - startIndex)
                throw new ArgumentOutOfRangeException(nameof(length));

            if (startIndex == 0)
            {
                RawChars = RawChars[length..];
            }
            else if (startIndex + length == pos)
            {
                RawChars = RawChars[..startIndex];
            }
            else
            {
                // Somewhere in the middle, this will be slow
                RawChars[(startIndex + length)..].CopyTo(RawChars[startIndex..]);
            }

            pos -= length;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void GrowAndAppend(char c)
        {
            Grow(1);
            Append(c);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void Grow(int requiredAdditionalCapacity)
        {
            Debug.Assert(requiredAdditionalCapacity > 0);

            char[] poolArray = ArrayPool<char>.Shared.Rent(Math.Max(pos + requiredAdditionalCapacity, RawChars.Length * 2));

            RawChars.CopyTo(poolArray);

            char[]? toReturn = arrayToReturnToPool;
            RawChars = arrayToReturnToPool = poolArray;
            
            if (toReturn is not null)
                ArrayPool<char>.Shared.Return(toReturn);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            char[]? toReturn = arrayToReturnToPool;
            this = default; // for safety, to avoid using pooled array if this instance is erroneously appended to again

            if (toReturn is not null)
                ArrayPool<char>.Shared.Return(toReturn);
        }
    }
}