using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MYPReader
{
    public sealed class MythicPackage : IDisposable
    {
        private static readonly MythicPackageFile empty = new();

        private readonly Dictionary<ulong, MythicPackageFile> filesByHash;
        private readonly ulong[] hashes;
        private readonly BinaryReader reader;

        public readonly MythicPackageHeader Header;
        public readonly FileInfo FileInfo;

        public int FileCount => filesByHash.Count;

        public MythicPackage(string fileName)
        {
            if (!File.Exists(fileName))
                throw new Exception($"Cannot find {Path.GetFileName(fileName)}!");

            filesByHash = new();

            FileStream stream = new(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            reader = new(stream);

            FileInfo = new FileInfo(fileName);
            Header = new MythicPackageHeader(reader);
            stream.Seek((long)Header.StartAddress, SeekOrigin.Begin);

            long nextBlock;

            do
            {
                nextBlock = ReadBlock(reader);
                stream.Seek(nextBlock, SeekOrigin.Begin);
            }
            while (nextBlock != 0);

            hashes = filesByHash.Keys.ToArray();
        }

        private long ReadBlock(BinaryReader reader)
        {
            int fileCount = reader.ReadInt32();
            long nextBlock = reader.ReadInt64();

            for (int index = 0; index < fileCount; index++)
            {
                MythicPackageFile file = new(reader);
                if (file is not { DataBlockAddress: > 0, UncompressedSize: > 0 })
                    continue;

                filesByHash.Add(file.FileHash, file);
            }

            return nextBlock;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref readonly MythicPackageFile SearchFile(string fileName)
        {
            ulong hash = HashFunctions.HashFileName(fileName);
            return ref SearchFile(hash);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref readonly MythicPackageFile SearchFile(ulong fileNameHash)
        {
            ref readonly MythicPackageFile file = ref CollectionsMarshal.GetValueRefOrNullRef(filesByHash, fileNameHash);

            if (Unsafe.IsNullRef(ref Unsafe.AsRef(in file)))
                return ref empty;

            return ref file;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte[] UnpackFile(string fileName)
        {
            ulong hash = HashFunctions.HashFileName(fileName);
            return UnpackFile(in CollectionsMarshal.GetValueRefOrNullRef(filesByHash, hash));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int UnpackFile(string fileName, ref byte[] buffer)
        {
            ulong hash = HashFunctions.HashFileName(fileName);
            return UnpackFile(in CollectionsMarshal.GetValueRefOrNullRef(filesByHash, hash), ref buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte[] UnpackFile(ulong fileNameHash)
        {
            return UnpackFile(in CollectionsMarshal.GetValueRefOrNullRef(filesByHash, fileNameHash));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int UnpackFile(ulong fileNameHash, ref byte[] buffer)
        {
            return UnpackFile(in CollectionsMarshal.GetValueRefOrNullRef(filesByHash, fileNameHash), ref buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte[] UnpackFile(in MythicPackageFile file)
        {
            if (Unsafe.IsNullRef(ref Unsafe.AsRef(in file)))
                return Array.Empty<byte>();

            byte[] toRet = new byte[file.UncompressedSize];
            file.Unpack(reader, ref toRet);

            return toRet;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int UnpackFile(in MythicPackageFile file, ref byte[] buffer)
        {
            if (Unsafe.IsNullRef(ref Unsafe.AsRef(in file)))
                return 0;

            return file.Unpack(reader, ref buffer);
        }

        public void Dispose()
        {
            reader.Dispose();
        }

        public MythicPackageEnumerator GetEnumerator()
        {
            return new(filesByHash, hashes);
        }

        public ref struct MythicPackageEnumerator
        {
            private static readonly MythicPackageFile empty = new();

            private readonly Dictionary<ulong, MythicPackageFile> files;
            private readonly ulong[] hashes;
            private int index;
            private ref readonly MythicPackageFile current;

            public ref readonly MythicPackageFile Current => ref current;

            public MythicPackageEnumerator(Dictionary<ulong, MythicPackageFile> files, ulong[] hashes)
            {
                this.files = files;
                this.hashes = hashes;
                index = -1;
                current = ref empty;
            }

            public bool MoveNext()
            {
                if (++index >= hashes.Length)
                    return false;

                current = ref CollectionsMarshal.GetValueRefOrNullRef(files, hashes[index]);
                return true;
            }

            public void Reset()
            {
                index = -1;
                current = ref empty;
            }
        }
    }
}
