using System.Runtime.InteropServices;

namespace MYPReader
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly struct MythicPackageHeader
    {
        private const uint magicNumber = 0x50594D; // MYP0

        public readonly int Version;
        public readonly uint Misc;
        public readonly ulong StartAddress;
        public readonly int BlockSize;
        public readonly int FileCount;

        public MythicPackageHeader(BinaryReader reader)
        {
            int id = reader.ReadInt32();

            if (id != magicNumber)
                throw new FormatException("This is not a Mythic Package file!");

            Version = reader.ReadInt32();
            Misc = reader.ReadUInt32();
            StartAddress = reader.ReadUInt64();
            BlockSize = reader.ReadInt32();
            FileCount = reader.ReadInt32();
        }
    }
}