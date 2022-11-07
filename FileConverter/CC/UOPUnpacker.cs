using System.Runtime.InteropServices;
using FileConverter.IO;

namespace FileConverter.CC
{
    internal sealed class UOPUnpacker
    {
        private const uint magicNumber = 0x50594D;

        private readonly FileReader reader;
        private readonly string pattern;

        public UOPUnpacker(FileReader reader, string pattern)
        {
            this.pattern = pattern;
            this.reader = reader;
        }

        public UOPFileContent[] Unpack()
            => Unpack((entry, i) => new UOPFileContent(entry.Address + entry.HeaderLength, entry.CompressedSize));

        public T[] Unpack<T>(Func<FileEntry, int, T> factory)
        {
            if (reader.ReadInt32() != magicNumber)
                throw new Exception("Invalid UOP file");

            reader.Skip(8); // version + timestamp

            Dictionary<ulong, FileEntry> entries = new();
            long nextBlock = reader.ReadInt64();

            while (nextBlock != 0)
            {
                reader.Seek(nextBlock);

                int filesCount = reader.ReadInt32();
                nextBlock = reader.ReadInt64();

                ReadBlock(entries, filesCount);
            }

            T[] files = new T[entries.Count];

            int added = 0;
            for (int i = 0; i < entries.Count; i++)
            {
                ulong hash = CreateHash(string.Format(pattern, i));

                if (!entries.TryGetValue(hash, out FileEntry entry))
                    continue;

                files[added++] = factory.Invoke(entry, i);
            }

            if (files.Length != added)
                Array.Resize(ref files, added);

            return files;
        }

        private void ReadBlock(Dictionary<ulong, FileEntry> entries, int filesCount)
        {
            for (int i = 0; i < filesCount; i++)
            {
                FileEntry entry = reader.Read<FileEntry>();

                if (entry.Address == 0)
                    continue;

                entries.Add(entry.Hash, entry);
            }
        }

        private static ulong CreateHash(string s)
        {
            uint eax, ecx, edx, ebx, esi, edi;
            eax = 0;
            ebx = edi = esi = (uint)s.Length + 0xDEADBEEF;

            int i;
            for (i = 0; i + 12 < s.Length; i += 12)
            {
                edi = (uint)(s[i + 7] << 24 | s[i + 6] << 16 | s[i + 5] << 8 | s[i + 4]) + edi;
                esi = (uint)(s[i + 11] << 24 | s[i + 10] << 16 | s[i + 9] << 8 | s[i + 8]) + esi;
                edx = (uint)(s[i + 3] << 24 | s[i + 2] << 16 | s[i + 1] << 8 | s[i]) - esi;
                edx = edx + ebx ^ esi >> 28 ^ esi << 4;
                esi += edi;
                edi = edi - edx ^ edx >> 26 ^ edx << 6;
                edx += esi;
                esi = esi - edi ^ edi >> 24 ^ edi << 8;
                edi += edx;
                ebx = edx - esi ^ esi >> 16 ^ esi << 16;
                esi += edi;
                edi = edi - ebx ^ ebx >> 13 ^ ebx << 19;
                ebx += esi;
                esi = esi - edi ^ edi >> 28 ^ edi << 4;
                edi += ebx;
            }

            if (s.Length - i > 0)
            {
                switch (s.Length - i)
                {
                    case 12: esi += (uint)s[i + 11] << 24; goto case 11;
                    case 11: esi += (uint)s[i + 10] << 16; goto case 10;
                    case 10: esi += (uint)s[i + 9] << 8; goto case 9;
                    case 9: esi += s[i + 8]; goto case 8;
                    case 8: edi += (uint)s[i + 7] << 24; goto case 7;
                    case 7: edi += (uint)s[i + 6] << 16; goto case 6;
                    case 6: edi += (uint)s[i + 5] << 8; goto case 5;
                    case 5: edi += s[i + 4]; goto case 4;
                    case 4: ebx += (uint)s[i + 3] << 24; goto case 3;
                    case 3: ebx += (uint)s[i + 2] << 16; goto case 2;
                    case 2: ebx += (uint)s[i + 1] << 8; goto case 1;
                    case 1: ebx += s[i]; break;
                }

                esi = (esi ^ edi) - (edi >> 18 ^ edi << 14);
                ecx = (esi ^ ebx) - (esi >> 21 ^ esi << 11);
                edi = (edi ^ ecx) - (ecx >> 7 ^ ecx << 25);
                esi = (esi ^ edi) - (edi >> 16 ^ edi << 16);
                edx = (esi ^ ecx) - (esi >> 28 ^ esi << 4);
                edi = (edi ^ edx) - (edx >> 18 ^ edx << 14);
                eax = (esi ^ edi) - (edi >> 8 ^ edi << 24);

                return (ulong)edi << 32 | eax;
            }

            return (ulong)esi << 32 | eax;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct FileEntry
    {
        public long Address;
        public int HeaderLength;
        public int CompressedSize;
        public int DecompressedSize;
        public ulong Hash;
        public uint ContentHash;
        public Flags Flags;
    }

    public enum Flags : short
    {
        Uncompressed = 0,
        Compressed = 1
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly struct UOPFileContent
    {
        public readonly long Address;
        public readonly int Length;

        public UOPFileContent(long address, int length)
        {
            Address = address;
            Length = length;
        }
    }
}
