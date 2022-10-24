using System;
using System.Collections.Generic;

namespace UOClient.IO
{
    internal class UOFileUop : UOFile
    {
        private const uint uopMagicNumber = 0x50594D;
        private readonly bool hasExtra;
        private readonly Dictionary<ulong, UOFileIndex> hashes = new();
        private readonly string pattern;

        public int TotalEntriesCount { get; private set; }

        public UOFileUop(string path, string pattern, bool hasextra = false)
            : base(path)
        {
            this.pattern = pattern;
            hasExtra = hasextra;
            Load();
        }

        public bool TryGetUOPData(ulong hash, out UOFileIndex data)
        {
            return hashes.TryGetValue(hash, out data);
        }

        protected override void Load()
        {
            base.Load();

            Seek(0);

            if (ReadUInt() != uopMagicNumber)
                throw new Exception("Bad uop file");

            Skip(8); // version + format_timestamp 
            long nextBlock = ReadLong();
            Skip(8); // block_size + count

            Seek(nextBlock);
            int total = 0;
            int realTotal = 0;

            do
            {
                int filesCount = ReadInt();
                nextBlock = ReadLong();
                total += filesCount;

                for (int i = 0; i < filesCount; i++)
                {
                    long offset = ReadLong();
                    int headerLength = ReadInt();
                    int compressedLength = ReadInt();
                    int decompressedLength = ReadInt();
                    ulong hash = ReadULong();
                    Skip(6); // data_hash + flag

                    if (offset == 0)
                        continue;

                    realTotal++;
                    offset += headerLength;

                    if (hasExtra)
                    {
                        long curpos = Position;
                        Seek(offset);
                        short extra1 = (short)ReadInt();
                        short extra2 = (short)ReadInt();

                        hashes.Add(hash,
                            new UOFileIndex(StartAddress, (uint)Length, offset + 8, compressedLength - 8, decompressedLength, extra1, extra2));

                        Seek(curpos);
                    }
                    else
                    {
                        hashes.Add(hash,
                            new UOFileIndex(StartAddress, (uint)Length, offset, compressedLength, decompressedLength));
                    }
                }

                Seek(nextBlock);
            }
            while (nextBlock != 0);

            TotalEntriesCount = realTotal;
        }

        public void ClearHashes()
        {
            hashes.Clear();
        }

        public override void Dispose()
        {
            ClearHashes();
            base.Dispose();
        }

        public override void FillEntries(ref UOFileIndex[] entries)
        {
            for (int i = 0; i < entries.Length; i++)
            {
                string file = string.Format(pattern, i);
                ulong hash = CreateHash(file);

                if (hashes.TryGetValue(hash, out UOFileIndex data))
                    entries[i] = data;
            }
        }

        public void FillEntries(ref UOFileIndex[] entries, bool clearHashes)
        {
            FillEntries(ref entries);

            if (clearHashes)
                ClearHashes();
        }

        internal static ulong CreateHash(string s)
        {
            uint eax, ecx, edx, ebx, esi, edi;
            eax = 0;
            ebx = edi = esi = (uint)s.Length + 0xDEADBEEF;

            int i;
            for (i = 0; i + 12 < s.Length; i += 12)
            {
                edi = (uint)((s[i + 7] << 24) | (s[i + 6] << 16) | (s[i + 5] << 8) | s[i + 4]) + edi;
                esi = (uint)((s[i + 11] << 24) | (s[i + 10] << 16) | (s[i + 9] << 8) | s[i + 8]) + esi;
                edx = (uint)((s[i + 3] << 24) | (s[i + 2] << 16) | (s[i + 1] << 8) | s[i]) - esi;
                edx = (edx + ebx) ^ (esi >> 28) ^ (esi << 4);
                esi += edi;
                edi = (edi - edx) ^ (edx >> 26) ^ (edx << 6);
                edx += esi;
                esi = (esi - edi) ^ (edi >> 24) ^ (edi << 8);
                edi += edx;
                ebx = (edx - esi) ^ (esi >> 16) ^ (esi << 16);
                esi += edi;
                edi = (edi - ebx) ^ (ebx >> 13) ^ (ebx << 19);
                ebx += esi;
                esi = (esi - edi) ^ (edi >> 28) ^ (edi << 4);
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

                esi = (esi ^ edi) - ((edi >> 18) ^ (edi << 14));
                ecx = (esi ^ ebx) - ((esi >> 21) ^ (esi << 11));
                edi = (edi ^ ecx) - ((ecx >> 7) ^ (ecx << 25));
                esi = (esi ^ edi) - ((edi >> 16) ^ (edi << 16));
                edx = (esi ^ ecx) - ((esi >> 28) ^ (esi << 4));
                edi = (edi ^ edx) - ((edx >> 18) ^ (edx << 14));
                eax = (esi ^ edi) - ((edi >> 8) ^ (edi << 24));

                return ((ulong)edi << 32) | eax;
            }

            return ((ulong)esi << 32) | eax;
        }
    }
}