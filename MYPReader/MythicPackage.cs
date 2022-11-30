using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MYPReader
{
    public sealed class MythicPackage : IDisposable
    {
        private readonly Dictionary<ulong, MythicPackageFile> filesByHash;
        private readonly ulong[] keys;
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

            long nextBlock = 0;

            do
            {
                nextBlock = ReadBlock(reader);
                stream.Seek(nextBlock, SeekOrigin.Begin);
            }
            while (nextBlock != 0);

            keys = filesByHash.Keys.ToArray();
        }

        private long ReadBlock(BinaryReader reader)
        {
            int fileCount = reader.ReadInt32();
            long nextBlock = reader.ReadInt64();

            for (int index = 0; index < fileCount; index++)
            {
                MythicPackageFile file = new(reader);
                if (file.DataBlockAddress == 0)
                    continue;

                filesByHash.Add(file.FileHash, file);
            }

            return nextBlock;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref readonly MythicPackageFile SearchFileName(string fileName)
        {
            ulong hash = HashFunctions.HashFileName(fileName);
            return ref CollectionsMarshal.GetValueRefOrNullRef(filesByHash, hash);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref readonly MythicPackageFile SearchFileNameHash(ulong fileNameHash)
        {
            return ref CollectionsMarshal.GetValueRefOrNullRef(filesByHash, fileNameHash);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte[] UnpackFile(string fileName)
        {
            ref readonly MythicPackageFile file = ref SearchFileName(fileName);
            return UnpackFile(in file);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte[] UnpackFile(ulong fileNameHash)
        {
            ref readonly MythicPackageFile file = ref SearchFileNameHash(fileNameHash);
            return UnpackFile(in file);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte[] UnpackFile(in MythicPackageFile file)
        {
            if (Unsafe.IsNullRef(ref Unsafe.AsRef(in file)))
                return Array.Empty<byte>();

            return file.Unpack(reader);
        }

        public void Dispose()
        {
            reader.Dispose();
        }

        public MythicPackageEnumerator GetEnumerator()
        {
            return new(filesByHash, keys);
        }

        public ref struct MythicPackageEnumerator
        {
            private static readonly MythicPackageFile empty = new();

            private readonly Dictionary<ulong, MythicPackageFile> files;
            private readonly ulong[] keys;
            private int index;
            private ref readonly MythicPackageFile current;

            public ref readonly MythicPackageFile Current => ref current;

            public MythicPackageEnumerator(Dictionary<ulong, MythicPackageFile> files, ulong[] keys)
            {
                this.files = files;
                this.keys = keys;
                index = -1;
                current = ref empty;
            }

            public bool MoveNext()
            {
                if (++index >= keys.Length)
                    return false;

                current = ref CollectionsMarshal.GetValueRefOrNullRef(files, keys[index]);
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
