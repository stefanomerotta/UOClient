using FileSystem.IO;
using GameData.Structures.Headers;
using System;
using System.IO;

namespace UOClient.Data
{
    internal sealed class TerrainTextureFile : IDisposable
    {
        private readonly PackageReader<TerrainTextureMetadata> reader;

        public TerrainTextureFile(string fileName)
        {
            FileStream stream = File.OpenRead(Path.Combine(Settings.FilePath, fileName));
            reader = new(stream);
        }

        public void GetTextureSize(int index, out byte dxtFormat, out ushort width, out ushort height)
        {
            ref readonly TerrainTextureMetadata metadata = ref reader.GetMetadata(index);

            dxtFormat = metadata.Format;
            width = metadata.Width;
            height = metadata.Height;
        }

        public byte[] ReadTexture(int index)
        {
            return reader.ReadArray(index);
        }

        public byte[] ReadTexture(int index, out byte dxtFormat, out ushort width, out ushort height)
        {
            byte[] data = reader.ReadArray(index, out TerrainTextureMetadata metadata);

            dxtFormat = metadata.Format;
            width = metadata.Width;
            height = metadata.Height;

            return data;
        }

        public int FillTexture(int index, Span<byte> data)
        {
            return reader.ReadSpan(index, data);
        }

        public int FillTexture(int index, Span<byte> data, out byte dxtFormat)
        {
            int count = reader.ReadSpan(index, data, out TerrainTextureMetadata metadata);

            dxtFormat = metadata.Format;

            return count;
        }

        public void Dispose()
        {
            reader.Dispose();
        }
    }
}
