namespace MYPReader
{
    public readonly struct MythicPackageHeader
    {
        public const int SupportedVersion = 5;

        public readonly int Version;
        public readonly uint Misc;
        public readonly ulong StartAddress;
        public readonly int BlockSize;
        public readonly int FileCount;

        public MythicPackageHeader(BinaryReader reader)
        {
            byte[] id = reader.ReadBytes(4);

            if (id[0] != 'M' || id[1] != 'Y' || id[2] != 'P' || id[3] != 0)
                throw new FormatException("This is not a Mythic Package file!");

            Version = reader.ReadInt32();

            if (Version > SupportedVersion)
                throw new FormatException("Unsupported version!");

            Misc = reader.ReadUInt32();
            StartAddress = reader.ReadUInt64();
            BlockSize = reader.ReadInt32();
            FileCount = reader.ReadInt32();
        }
    }
}