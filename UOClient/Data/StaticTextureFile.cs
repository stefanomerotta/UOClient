using FileSystem.IO;
using GameData.Structures.Headers;
using System;
using System.IO;

namespace UOClient.Data
{
    internal sealed class StaticTextureFile : IDisposable
    {
        private readonly PackageReader<StaticTextureMetadata> reader;

        public StaticTextureFile(string fileName)
        {
            FileStream stream = File.OpenRead(Path.Combine(Settings.FilePath, fileName));
            reader = new(stream);
        }

        public void GetTextureSize(int index, out ushort width, out ushort height)
        {
            ref readonly StaticTextureMetadata metadata = ref reader.GetMetadata(index);

            width = metadata.Width;
            height = metadata.Height;
        }

        public byte[] ReadTexture(int index)
        {
            return reader.ReadArray(index);
        }

        public byte[] ReadTexture(int index, out ushort width, out ushort height)
        {
            byte[] data = reader.ReadArray(index, out StaticTextureMetadata metadata);

            width = metadata.Width;
            height = metadata.Height;

            return data;
        }

        public int FillTexture(int index, Span<byte> data)
        {
            return reader.ReadSpan(index, data);
        }

        public void Dispose()
        {
            reader.Dispose();
        }
    }
}
