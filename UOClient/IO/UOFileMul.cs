namespace UOClient.IO
{
    internal class UOFileMul : UOFile
    {
        private readonly UOFileIdxMul? idxFile;

        public UOFile? IdxFile => idxFile;

        public UOFileMul(string file, string idxfile)
            : this(file)
        {
            idxFile = new UOFileIdxMul(idxfile);
        }

        public UOFileMul(string file)
            : base(file)
        {
            Load();
        }

        public override void FillEntries(ref UOFileIndex[] entries)
        {
            UOFile file = idxFile ?? (UOFile)this;

            int count = (int)file.Length / 12;
            entries = new UOFileIndex[count];

            for (int i = 0; i < count; i++)
            {
                ref UOFileIndex e = ref entries[i];
                e.Address = StartAddress;   // .mul mmf address
                e.FileSize = (uint)Length;  // .mul mmf length
                e.Offset = file.ReadUInt(); // .idx offset
                e.Length = file.ReadInt();  // .idx length
                e.DecompressedLength = 0;   // UNUSED HERE --> .UOP

                int size = file.ReadInt();

                if (size > 0)
                {
                    e.Width = (short)(size >> 16);
                    e.Height = (short)(size & 0xFFFF);
                }
            }
        }

        public override void Dispose()
        {
            idxFile?.Dispose();
            base.Dispose();
        }

        private sealed class UOFileIdxMul : UOFile
        {
            public UOFileIdxMul(string idxpath)
                : base(idxpath)
            {
                Load();
            }

            public override void FillEntries(ref UOFileIndex[] entries)
            { }
        }
    }
}