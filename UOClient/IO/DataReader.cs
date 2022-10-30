using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UOClient.Utilities;

namespace UOClient.IO
{
    /// <summary>
    ///     A fast Little Endian data reader.
    /// </summary>
    internal unsafe class DataReader
    {
        private byte* data;
        private GCHandle handle;

        internal long Position { get; set; }
        internal long Length { get; private set; }
        internal IntPtr StartAddress => (IntPtr)data;
        internal IntPtr PositionAddress => (IntPtr)(data + Position);
        public bool IsEOF => Position >= Length;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReleaseData()
        {
            if (handle.IsAllocated)
                handle.Free();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void SetData(byte* data, long length)
        {
            ReleaseData();

            this.data = data;
            Length = length;
            Position = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void SetData(byte[] data, long length)
        {
            ReleaseData();
            handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            this.data = (byte*)handle.AddrOfPinnedObject();
            Length = length;
            Position = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void SetData(IntPtr data, long length)
        {
            SetData((byte*)data, length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void SetData(IntPtr data)
        {
            SetData((byte*)data, Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Seek(long idx)
        {
            Position = idx;
            EnsureSize(0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Seek(int idx)
        {
            Position = idx;
            EnsureSize(0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Skip(int count)
        {
            EnsureSize(count);
            Position += count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal byte ReadByte()
        {
            EnsureSize(1);

            return data[Position++];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal sbyte ReadSByte()
        {
            return (sbyte)ReadByte();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool ReadBool()
        {
            return ReadByte() != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal short ReadShort()
        {
            EnsureSize(2);

            short v = *(short*)(data + Position);
            Position += 2;

            return v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ushort ReadUShort()
        {
            EnsureSize(2);

            ushort v = *(ushort*)(data + Position);
            Position += 2;

            return v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal int ReadInt()
        {
            EnsureSize(4);

            int v = *(int*)(data + Position);

            Position += 4;

            return v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal uint ReadUInt()
        {
            EnsureSize(4);

            uint v = *(uint*)(data + Position);
            Position += 4;

            return v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal long ReadLong()
        {
            EnsureSize(8);

            long v = *(long*)(data + Position);
            Position += 8;

            return v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ulong ReadULong()
        {
            EnsureSize(8);

            ulong v = *(ulong*)(data + Position);
            Position += 8;

            return v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal byte[] ReadArray(int count)
        {
            EnsureSize(count);

            byte[] data = new byte[count];

            fixed (byte* ptr = data)
            {
                Buffer.MemoryCopy(&this.data[Position], ptr, count, count);
            }

            Position += count;

            return data;
        }

        internal string ReadASCII(int size)
        {
            EnsureSize(size);

            Span<char> span = stackalloc char[size];
            ValueStringBuilder sb = new ValueStringBuilder(span);

            for (int i = 0; i < size; i++)
            {
                char c = (char)ReadByte();

                if (c != 0)
                    sb.Append(c);
            }

            string ss = sb.ToString();
            sb.Dispose();

            return ss;
        }

        [Conditional("DEBUG")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureSize(int size)
        {
            if (Position + size > Length)
                throw new IndexOutOfRangeException();
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort ReadUShortReversed()
        {
            EnsureSize(2);

            return (ushort)((ReadByte() << 8) | ReadByte());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint ReadUIntReversed()
        {
            EnsureSize(4);

            return (uint)((ReadByte() << 24) | (ReadByte() << 16) | (ReadByte() << 8) | ReadByte());
        }
    }
}