using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static System.Buffers.Binary.BinaryPrimitives;

namespace Common.Buffers
{
    /// <summary>
    /// Represents simple memory reader backed by <see cref="ReadOnlySpan{byte}"/>.
    /// </summary>
    public ref struct ByteSpanReader
    {
        private readonly ReadOnlySpan<byte> span;
        private int position;

        /// <summary>
        /// Gets the number of consumed elements.
        /// </summary>
        public readonly int ConsumedCount => position;

        /// <summary>
        /// Gets the number of unread elements.
        /// </summary>
        public readonly int RemainingCount => span.Length - position;

        /// <summary>
        /// Gets underlying span.
        /// </summary>
        public readonly ReadOnlySpan<byte> Span => span;

        /// <summary>
        /// Gets the span over consumed elements.
        /// </summary>
        public readonly ReadOnlySpan<byte> ConsumedSpan => span[..position];

        /// <summary>
        /// Gets the remaining part of the span.
        /// </summary>
        public readonly ReadOnlySpan<byte> RemainingSpan => span.Slice(position);

        /// <summary>
        /// Gets the element at the current position in the
        /// underlying memory block.
        /// </summary>
        /// <exception cref="InvalidOperationException">The position of this reader is out of range.</exception>
        public readonly ref readonly byte Current
        {
            get
            {
                if ((uint)position >= (uint)span.Length)
                    throw new InvalidOperationException();

                return ref Unsafe.Add(ref MemoryMarshal.GetReference(span), position);
            }
        }

        /// <summary>
        /// Initializes a new memory reader.
        /// </summary>
        /// <param name="span">The span to read from.</param>
        public ByteSpanReader(ReadOnlySpan<byte> span)
        {
            this.span = span;
            position = 0;
        }

        /// <summary>
        /// Advances the position of this reader.
        /// </summary>
        /// <param name="count">The number of consumed elements.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is greater than the available space in the rest of the memory block.</exception>
        public void Advance(int count)
        {
            if (count < 0 || position > span.Length - count)
                ThrowCountOutOfRangeException();

            position += count;
        }

        public void Seek(int position)
        {
            if (position < 0 || position > span.Length)
                ThrowCountOutOfRangeException();

            this.position = position;
        }

        /// <summary>
        /// Moves the reader back the specified number of items.
        /// </summary>
        /// <param name="count">The number of items.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is less than zero or greater than <see cref="ConsumedCount"/>.</exception>
        public void Rewind(int count)
        {
            if ((uint)count > (uint)position)
                ThrowCountOutOfRangeException();

            position -= count;
        }

        /// <summary>
        /// Sets reader position to the first element.
        /// </summary>
        public void Reset() => position = 0;

        /// <summary>
        /// Copies elements from the underlying span.
        /// </summary>
        /// <param name="output">The span used to write elements from the underlying span.</param>
        /// <returns><see langword="true"/> if size of <paramref name="output"/> is less than or equal to <see cref="RemainingCount"/>; otherwise, <see langword="false"/>.</returns>
        public bool TryRead(Span<byte> output)
            => TryRead(output.Length, out var input) && input.TryCopyTo(output);

        /// <summary>
        /// Reads the portion of data from the underlying span.
        /// </summary>
        /// <param name="count">The number of elements to read from the underlying span.</param>
        /// <param name="result">The segment of the underlying span.</param>
        /// <returns><see langword="true"/> if <paramref name="count"/> is less than or equal to <see cref="RemainingCount"/>; otherwise <see langword="false"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is negative.</exception>
        public bool TryRead(int count, out ReadOnlySpan<byte> result)
        {
            if (count < 0)
                ThrowCountOutOfRangeException();

            int newLength = position + count;

            if ((uint)newLength <= (uint)span.Length)
            {
                result = span.Slice(position, count);
                position = newLength;
                return true;
            }

            result = default;
            return false;
        }

        /// <summary>
        /// Reads the portion of data from the underlying span.
        /// </summary>
        /// <param name="count">The number of elements to read from the underlying span.</param>
        /// <param name="result">The segment of the underlying span.</param>
        /// <returns><see langword="true"/> if <paramref name="count"/> is less than or equal to <see cref="RemainingCount"/>; otherwise <see langword="false"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is negative.</exception>
        public bool TryRead<T>(int count, out ReadOnlySpan<T> result) where T : struct
        {
            if (count < 0)
                ThrowCountOutOfRangeException();

            int byteCount = count * Unsafe.SizeOf<T>();
            int newLength = position + byteCount;

            if ((uint)newLength <= (uint)span.Length)
            {
                result = MemoryMarshal.Cast<byte, T>(span.Slice(position, byteCount));
                position = newLength;
                return true;
            }

            result = default;
            return false;
        }

        /// <summary>
        /// Reads single element from the underlying span.
        /// </summary>
        /// <param name="result">The obtained element.</param>
        /// <returns><see langword="true"/> if element is obtained successfully; otherwise, <see langword="false"/>.</returns>
        public bool TryReadByte([MaybeNullWhen(false)] out byte result)
        {
            var newLength = position + 1;

            if ((uint)newLength <= (uint)span.Length)
            {
                result = Unsafe.Add(ref MemoryMarshal.GetReference(span), position);
                position = newLength;
                return true;
            }

            result = default;
            return false;
        }

        /// <summary>
        /// Copies elements from the underlying span.
        /// </summary>
        /// <param name="output">The span used to write elements from the underlying span.</param>
        /// <returns>The number of obtained elements.</returns>
        public int Read(scoped Span<byte> output)
        {
            int length = Math.Min(RemainingCount, output.Length);

            span.Slice(position, length).CopyTo(output);
            position += length;

            return length;
        }

        public T Read<T>() where T : struct
        {
            T result = MemoryMarshal.Read<T>(RemainingSpan);
            position += Unsafe.SizeOf<T>();

            return result;
        }

        public ref readonly T ReadRef<T>() where T : struct
        {
            ref readonly T result = ref MemoryMarshal.AsRef<T>(RemainingSpan);
            position += Unsafe.SizeOf<T>();

            return ref result;
        }

        /// <summary>
        /// Reads single element from the underlying span.
        /// </summary>
        /// <returns>The element obtained from the span.</returns>
        /// <exception cref="InternalBufferOverflowException">The end of memory block is reached.</exception>
        public byte ReadByte()
        {
            if (!TryReadByte(out var result))
                ThrowInternalBufferOverflowException();

            return result;
        }

        /// <summary>
        /// Reads the portion of data from the underlying span.
        /// </summary>
        /// <param name="count">The number of elements to read from the underlying span.</param>
        /// <returns>The portion of data within the underlying span.</returns>
        /// <exception cref="InternalBufferOverflowException"><paramref name="count"/> is greater than <see cref="RemainingCount"/>.</exception>
        public ReadOnlySpan<byte> ReadSpan(int count)
        {
            if (!TryRead(count, out ReadOnlySpan<byte> result))
                ThrowInternalBufferOverflowException();

            return result;
        }

        /// <summary>
        /// Reads the portion of data from the underlying span.
        /// </summary>
        /// <param name="count">The number of elements to read from the underlying span.</param>
        /// <returns>The portion of data within the underlying span.</returns>
        /// <exception cref="InternalBufferOverflowException"><paramref name="count"/> is greater than <see cref="RemainingCount"/>.</exception>
        public ReadOnlySpan<T> ReadSpan<T>(int count) where T : struct
        {
            if (!TryRead(count, out ReadOnlySpan<T> result))
                ThrowInternalBufferOverflowException();

            return result;
        }

        [DoesNotReturn]
        [StackTraceHidden]
        private static void ThrowInternalBufferOverflowException() => throw new InternalBufferOverflowException();

        [DoesNotReturn]
        [StackTraceHidden]
        private static void ThrowCountOutOfRangeException() => throw new ArgumentOutOfRangeException("count");

        /// <summary>
        /// Reads the rest of the memory block.
        /// </summary>
        /// <returns>The rest of the memory block.</returns>
        public ReadOnlySpan<byte> ReadToEnd()
        {
            ReadOnlySpan<byte> result = RemainingSpan;
            position = span.Length;
            return result;
        }

        /// <summary>
        /// Decodes 16-bit signed integer.
        /// </summary>
        /// <param name="isLittleEndian"><see langword="true"/> to use little-endian encoding; <see langword="false"/> to use big-endian encoding.</param>
        /// <returns>The decoded value.</returns>
        /// <exception cref="InternalBufferOverflowException">The end of memory block is reached.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short ReadInt16(bool isLittleEndian = true)
        {
            short result = Read<short>();

            if (isLittleEndian != BitConverter.IsLittleEndian)
                result = ReverseEndianness(result);

            return result;
        }

        /// <summary>
        /// Decodes 16-bit unsigned integer.
        /// </summary>
        /// <param name="isLittleEndian"><see langword="true"/> to use little-endian encoding; <see langword="false"/> to use big-endian encoding.</param>
        /// <returns>The decoded value.</returns>
        /// <exception cref="InternalBufferOverflowException">The end of memory block is reached.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public ushort ReadUInt16(bool isLittleEndian = true)
        {
            ushort result = Read<ushort>();

            if (isLittleEndian != BitConverter.IsLittleEndian)
                result = ReverseEndianness(result);

            return result;
        }

        /// <summary>
        /// Decodes 32-bit signed integer.
        /// </summary>
        /// <param name="isLittleEndian"><see langword="true"/> to use little-endian encoding; <see langword="false"/> to use big-endian encoding.</param>
        /// <returns>The decoded value.</returns>
        /// <exception cref="InternalBufferOverflowException">The end of memory block is reached.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReadInt32(bool isLittleEndian = true)
        {
            int result = Read<int>();

            if (isLittleEndian != BitConverter.IsLittleEndian)
                result = ReverseEndianness(result);

            return result;
        }

        /// <summary>
        /// Decodes 32-bit unsigned integer.
        /// </summary>
        /// <param name="isLittleEndian"><see langword="true"/> to use little-endian encoding; <see langword="false"/> to use big-endian encoding.</param>
        /// <returns>The decoded value.</returns>
        /// <exception cref="InternalBufferOverflowException">The end of memory block is reached.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public uint ReadUInt32(bool isLittleEndian = true)
        {
            uint result = Read<uint>();

            if (isLittleEndian != BitConverter.IsLittleEndian)
                result = ReverseEndianness(result);

            return result;
        }

        /// <summary>
        /// Decodes 64-bit signed integer.
        /// </summary>
        /// <param name="isLittleEndian"><see langword="true"/> to use little-endian encoding; <see langword="false"/> to use big-endian encoding.</param>
        /// <returns>The decoded value.</returns>
        /// <exception cref="InternalBufferOverflowException">The end of memory block is reached.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long ReadInt64(bool isLittleEndian = true)
        {
            long result = Read<long>();

            if (isLittleEndian != BitConverter.IsLittleEndian)
                result = ReverseEndianness(result);

            return result;
        }

        /// <summary>
        /// Decodes 64-bit unsigned integer.
        /// </summary>
        /// <param name="isLittleEndian"><see langword="true"/> to use little-endian encoding; <see langword="false"/> to use big-endian encoding.</param>
        /// <returns>The decoded value.</returns>
        /// <exception cref="InternalBufferOverflowException">The end of memory block is reached.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public ulong ReadUInt64(bool isLittleEndian = true)
        {
            ulong result = Read<ulong>();

            if (isLittleEndian != BitConverter.IsLittleEndian)
                result = ReverseEndianness(result);

            return result;
        }

        /// <summary>
        /// Decodes single-precision floating-point number.
        /// </summary>
        /// <param name="isLittleEndian"><see langword="true"/> to use little-endian encoding; <see langword="false"/> to use big-endian encoding.</param>
        /// <returns>The decoded value.</returns>
        /// <exception cref="InternalBufferOverflowException">The end of memory block is reached.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float ReadSingle(bool isLittleEndian = true)
            => BitConverter.Int32BitsToSingle(ReadInt32(isLittleEndian));

        /// <summary>
        /// Decodes double-precision floating-point number.
        /// </summary>
        /// <param name="isLittleEndian"><see langword="true"/> to use little-endian encoding; <see langword="false"/> to use big-endian encoding.</param>
        /// <returns>The decoded value.</returns>
        /// <exception cref="InternalBufferOverflowException">The end of memory block is reached.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double ReadDouble(bool isLittleEndian = true)
            => BitConverter.Int64BitsToDouble(ReadInt64(isLittleEndian));

        /// <summary>
        /// Decodes half-precision floating-point number.
        /// </summary>
        /// <param name="isLittleEndian"><see langword="true"/> to use little-endian encoding; <see langword="false"/> to use big-endian encoding.</param>
        /// <returns>The decoded value.</returns>
        /// <exception cref="InternalBufferOverflowException">The end of memory block is reached.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Half ReadHalf(bool isLittleEndian = true)
            => BitConverter.Int16BitsToHalf(ReadInt16(isLittleEndian));

        /// <summary>
        /// Decodes boolean.
        /// </summary>
        /// <returns>The decoded value.</returns>
        /// <exception cref="InternalBufferOverflowException">The end of memory block is reached.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ReadBoolean()
            => ReadByte() != 0;

        /// <summary>
        /// Gets the textual representation of the written content.
        /// </summary>
        /// <returns>The textual representation of the written content.</returns>
        public readonly override string ToString() => ConsumedSpan.ToString();
    }
}