using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Common.Buffers
{
    /// <summary>
    /// Represents simple memory writer backed by <see cref="Span{byte}"/>.
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
    public ref struct ByteSpanWriter
    {
        private readonly Span<byte> span;
        private int position;

        /// <summary>
        /// Gets the available space in the underlying span.
        /// </summary>
        public readonly int FreeCapacity => span.Length - position;

        /// <summary>
        /// Gets the number of occupied elements in the underlying span.
        /// </summary>
        public readonly int WrittenCount => position;

        /// <summary>
        /// Gets the remaining part of the span.
        /// </summary>
        public readonly Span<byte> RemainingSpan => span[position..];

        /// <summary>
        /// Gets the span over written elements.
        /// </summary>
        /// <value>The segment of underlying span containing written elements.</value>
        public readonly Span<byte> WrittenSpan => span[..position];

        /// <summary>
        /// Gets underlying span.
        /// </summary>
        public readonly Span<byte> Span => span;

        /// <summary>
        /// Gets the element at the current position in the
        /// underlying memory block.
        /// </summary>
        /// <exception cref="InvalidOperationException">The position of this writer is out of range.</exception>
        public readonly ref byte Current
        {
            get
            {
                if (position >= span.Length)
                    throw new InvalidOperationException();

                return ref Unsafe.Add(ref MemoryMarshal.GetReference(span), position);
            }
        }

        /// <summary>
        /// Initializes a new memory writer.
        /// </summary>
        /// <param name="span">The span used to write elements.</param>
        public ByteSpanWriter(Span<byte> span)
        {
            this.span = span;
            position = 0;
        }

        /// <summary>
        /// Advances the position of this writer.
        /// </summary>
        /// <param name="count">The number of written elements.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is greater than the available space in the rest of the memory block.</exception>
        public void Advance(int count)
        {
            if (count < 0 || position > span.Length - count)
                ThrowCountOutOfRangeException();

            position += count;
        }

        /// <summary>
        /// Moves the writer back the specified number of items.
        /// </summary>
        /// <param name="count">The number of items.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is less than zero or greater than <see cref="WrittenCount"/>.</exception>
        public void Rewind(int count)
        {
            if ((uint)count > (uint)position)
                ThrowCountOutOfRangeException();

            position -= count;
        }

        /// <summary>
        /// Sets writer position to the first element.
        /// </summary>
        public void Reset() => position = 0;

        /// <summary>
        /// Copies the elements to the underlying span.
        /// </summary>
        /// <param name="input">The span to copy from.</param>
        /// <returns>
        /// <see langword="true"/> if all elements are copied successfully;
        /// <see langword="false"/> if remaining space in the underlying span is not enough to place all elements from <paramref name="input"/>.
        /// </returns>
        public bool TryWrite(ReadOnlySpan<byte> input)
        {
            if (!input.TryCopyTo(span.Slice(position)))
                return false;

            position += input.Length;
            return true;
        }

        /// <summary>
        /// Copies the elements to the underlying span.
        /// </summary>
        /// <param name="input">The span of elements to copy from.</param>
        /// <returns>The number of written elements.</returns>
        public int Write(ReadOnlySpan<byte> input)
        {
            int length = Math.Min(RemainingSpan.Length, input.Length);

            input[..length].CopyTo(RemainingSpan);
            position += length;

            return length;
        }

        /// <summary>
        /// Copies the element to the underlying span.
        /// </summary>
        /// <param name="input">The item to copy from.</param>
        /// <returns>
        /// <see langword="true"/> if the element has been copied successfully;
        /// <see langword="false"/> if remaining space in the underlying span is not enough to place the element from <paramref name="input"/>.
        /// </returns>
        public bool TryWrite<T>(in T input)
            where T : struct
        {
            ReadOnlySpan<T> converted = new(in input);

            if (!MemoryMarshal.AsBytes(converted).TryCopyTo(span[position..]))
                return false;

            position += Unsafe.SizeOf<T>();
            return true;
        }

        /// <summary>
        /// Copies the element to the underlying span.
        /// </summary>
        /// <param name="input">The element to copy from.</param>
        public void Write<T>(in T input)
            where T : struct
        {
            ReadOnlySpan<T> converted = new(in input);
            MemoryMarshal.AsBytes(converted).CopyTo(span[position..]);
                
            position += Unsafe.SizeOf<T>();
        }

        /// <summary>
        /// Puts single element into the underlying span.
        /// </summary>
        /// <param name="item">The item to place.</param>
        /// <returns>
        /// <see langword="true"/> if item has beem placed successfully;
        /// <see langword="false"/> if remaining space in the underlying span is not enough to place the item.
        /// </returns>
        public bool TryWriteByte(byte item)
        {
            var newLength = position + 1;
            if ((uint)newLength > (uint)span.Length)
                return false;

            Unsafe.Add(ref MemoryMarshal.GetReference(span), position) = item;
            position = newLength;
            return true;
        }

        /// <summary>
        /// Puts single element into the underlying span.
        /// </summary>
        /// <param name="item">The item to place.</param>
        /// <exception cref="InternalBufferOverflowException">Remaining space in the underlying span is not enough to place the item.</exception>
        public void WriteByte(byte item)
        {
            if (!TryWriteByte(item))
                ThrowInternalBufferOverflowException();
        }

        /// <summary>
        /// Obtains the portion of underlying span and marks it as written.
        /// </summary>
        /// <param name="count">The size of the segment.</param>
        /// <param name="segment">The portion of the underlying span.</param>
        /// <returns>
        /// <see langword="true"/> if segment is obtained successfully;
        /// <see langword="false"/> if remaining space in the underlying span is not enough to place <paramref name="count"/> elements.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is negative.</exception>
        public bool TrySlide(int count, out Span<byte> segment)
        {
            if (count < 0)
                ThrowCountOutOfRangeException();

            var newLength = position + count;
            if ((uint)newLength <= (uint)span.Length)
            {
                segment = span.Slice(position, count);
                position = newLength;
                return true;
            }

            segment = default;
            return false;
        }

        /// <summary>
        /// Obtains the portion of underlying span and marks it as written.
        /// </summary>
        /// <param name="count">The size of the segment.</param>
        /// <returns>The portion of the underlying span.</returns>
        /// <exception cref="InternalBufferOverflowException">Remaining space in the underlying span is not enough to place <paramref name="count"/> elements.</exception>
        public Span<byte> Slide(int count)
        {
            if (!TrySlide(count, out var result))
                ThrowInternalBufferOverflowException();

            return result;
        }

        [DoesNotReturn]
        [StackTraceHidden]
        private static void ThrowInternalBufferOverflowException() 
            => throw new InternalBufferOverflowException("Not Enough Memory");

        [DoesNotReturn]
        [StackTraceHidden]
        private static void ThrowCountOutOfRangeException() 
            => throw new ArgumentOutOfRangeException("count");

        /// <summary>
        /// Gets the textual representation of the written content.
        /// </summary>
        /// <returns>The textual representation of the written content.</returns>
        public readonly override string ToString() => WrittenSpan.ToString();
    }
}