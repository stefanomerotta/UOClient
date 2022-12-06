using Microsoft.Win32.SafeHandles;
using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace UOClient.Old.IO
{
    internal unsafe sealed class FileReader : IDisposable
    {
        private readonly MemoryMappedFile file;
        private readonly MemoryMappedViewAccessor accessor;
        private readonly SafeMemoryMappedViewHandle handle;
        private readonly byte* pointer;
        private ulong position;

        public FileReader(string filePath)
        {
            FileInfo fileInfo = new(filePath);

            file = MemoryMappedFile.CreateFromFile(filePath, FileMode.Open, null, 0, MemoryMappedFileAccess.Read);
            accessor = file.CreateViewAccessor(0, fileInfo.Length, MemoryMappedFileAccess.Read);
            handle = accessor.SafeMemoryMappedViewHandle;

            pointer = null;
            accessor.SafeMemoryMappedViewHandle.AcquirePointer(ref pointer);
        }

        public long ReadInt64() => Read<long>(8);
        public ulong ReadUInt64() => Read<ulong>(8);
        public int ReadInt32() => Read<int>(4);
        public uint ReadUInt32() => Read<uint>(4);
        public short ReadInt16() => Read<short>(2);
        public ushort ReadUInt16() => Read<ushort>(2);
        public byte ReadByte() => Read<byte>(1);
        public sbyte ReadSByte() => Read<sbyte>(1);

        public unsafe T Read<T>() where T : struct
        {
            T toRet = handle.Read<T>(position);
            position += (uint)Unsafe.SizeOf<T>();
            return toRet;
        }

        public void ReadSpan<T>(Span<T> span) where T : struct
        {
            Span<byte> byteSpan = MemoryMarshal.Cast<T, byte>(span);

            handle.ReadSpan(position, byteSpan);
            position += (uint)byteSpan.Length;
        }

        public void Seek(ulong position)
        {
            this.position = position;
        }

        public void Seek(long position)
        {
            this.position = (ulong)position;
        }

        public void Skip(uint length) => position += length;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private T Read<T>(uint length) where T : unmanaged
        {
            T toRet = *(T*)(pointer + position);
            position += length;
            return toRet;
        }

        public void Dispose()
        {
            handle.Dispose();
            accessor.Dispose();
            file.Dispose();
        }
    }
}
