using MYPReader.Enums;
using System.Buffers;
using System.Runtime.InteropServices;

namespace MYPReader
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly record struct MythicPackageFile
    {
        public readonly long DataBlockAddress;
        public readonly ulong FileHash;
        public readonly uint CompressedSize;
        public readonly int UncompressedSize;
        public readonly CompressionFlag Compression;

        public MythicPackageFile(BinaryReader reader)
        {
            DataBlockAddress = reader.ReadInt64();
            DataBlockAddress += reader.ReadInt32(); // HeaderLength
            CompressedSize = reader.ReadUInt32();
            UncompressedSize = (int)reader.ReadUInt32();
            FileHash = reader.ReadUInt64();
            reader.ReadUInt32(); // DataBlockHash
            Compression = (CompressionFlag)reader.ReadInt16();

            if (!Enum.IsDefined(Compression))
                throw new InvalidCompressionException(Compression);
        }

        internal int Unpack(BinaryReader reader, ref byte[] buffer)
        {
            reader.BaseStream.Seek(DataBlockAddress, SeekOrigin.Begin);

            if (buffer.Length < UncompressedSize)
                buffer = new byte[UncompressedSize];

            if (Compression is CompressionFlag.None)
            {
                reader.Read(buffer, 0, UncompressedSize);
            }
            else
            {
                byte[] sourceData = ArrayPool<byte>.Shared.Rent((int)CompressedSize);
                reader.Read(sourceData, 0, (int)CompressedSize);

                ZLibError error = Zlib.Unzip(buffer, sourceData);

                ArrayPool<byte>.Shared.Return(sourceData);

                if (error != ZLibError.Okay)
                    throw new CompressionException(error);
            }

            return UncompressedSize;
        }
    }
}
