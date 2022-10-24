using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.CompilerServices;

namespace UOClient.IO
{
    internal unsafe class UOFile : DataReader
    {
        private protected MemoryMappedViewAccessor accessor;
        private protected MemoryMappedFile file;

        public string FilePath { get; }

        public UOFile(string filepath, bool loadFile = false)
        {
            FilePath = filepath;

            if (loadFile)
                Load();
        }

        protected virtual void Load()
        {
            FileInfo fileInfo = new(FilePath);

            if (!fileInfo.Exists)
                return;

            long size = fileInfo.Length;

            if (size > 0)
            {
                file = MemoryMappedFile.CreateFromFile
                (
                    File.Open(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite),
                    null,
                    0,
                    MemoryMappedFileAccess.Read,
                    HandleInheritability.None,
                    false
                );

                accessor = file.CreateViewAccessor(0, size, MemoryMappedFileAccess.Read);

                byte* ptr = null;

                try
                {
                    accessor.SafeMemoryMappedViewHandle.AcquirePointer(ref ptr);
                    SetData(ptr, (long)accessor.SafeMemoryMappedViewHandle.ByteLength);
                }
                catch
                {
                    accessor.SafeMemoryMappedViewHandle.ReleasePointer();

                    throw new Exception("Something goes wrong...");
                }
            }
        }

        public virtual void FillEntries(ref UOFileIndex[] entries)
        { }

        public virtual void Dispose()
        {
            accessor.SafeMemoryMappedViewHandle.ReleasePointer();
            accessor.Dispose();
            file.Dispose();
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Fill(ref byte[] buffer, int count)
        {
            byte* ptr = (byte*)PositionAddress;

            for (int i = 0; i < count; i++)
            {
                buffer[i] = ptr[i];
            }

            Position += count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal T[] ReadArray<T>(int count) where T : struct
        {
            T[] t = ReadArray<T>(Position, count);
            Position += Unsafe.SizeOf<T>() * count;

            return t;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private T[] ReadArray<T>(long position, int count) where T : struct
        {
            T[] array = new T[count];
            accessor.ReadArray(position, array, 0, count);

            return array;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal T ReadStruct<T>(long position) where T : struct
        {
            accessor.Read(position, out T s);

            return s;
        }
    }
}