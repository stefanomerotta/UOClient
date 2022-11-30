using MYPReader.Enums;

namespace MYPReader
{
    public readonly record struct MythicPackageFile
    {
        public readonly long DataBlockAddress;
        public readonly int DataBlockLength;
        public readonly uint DataBlockHash;
        public readonly CompressionFlag Compression;
        public readonly uint CompressedSize;
        public readonly uint DecompressedSize;
        public readonly ulong FileHash;

        public MythicPackageFile(BinaryReader reader)
        {
            DataBlockAddress = reader.ReadInt64();
            DataBlockLength = reader.ReadInt32();
            CompressedSize = reader.ReadUInt32();
            DecompressedSize = reader.ReadUInt32();
            FileHash = reader.ReadUInt64();

            DataBlockHash = reader.ReadUInt32();
            Compression = (CompressionFlag)reader.ReadInt16();

            if (Compression is not CompressionFlag.None and not CompressionFlag.Zlib)
                throw new InvalidCompressionException(Compression);
        }

        internal byte[] Unpack(BinaryReader reader)
        {
            reader.BaseStream.Seek(DataBlockAddress + DataBlockLength, SeekOrigin.Begin);
            byte[] sourceData = new byte[CompressedSize];
            reader.Read(sourceData, 0, (int)CompressedSize);

            if (Compression is CompressionFlag.None)
                return sourceData;

            byte[] destData = new byte[DecompressedSize];
            ZLibError error = Zlib.Unzip(destData, sourceData);

            return error != ZLibError.Okay ? throw new CompressionException(error) : destData;
        }
    }
}
