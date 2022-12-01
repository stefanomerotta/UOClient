using FileSystem.IO;
using GameData.Structures.Headers;
using System;
using System.IO;

namespace UOClient.Data
{
    internal sealed class TextureFile : IDisposable
    {
        private readonly PackageReader<TextureMetadata> reader;

        public TextureFile(string fileName)
        {
            FileStream stream = File.Open(Path.Combine(Settings.FilePath, fileName), FileMode.Open);
            reader = new(stream);
        }

        public void GetTextureSize(int index, out ushort width, out ushort height)
        {
            ref readonly TextureMetadata metadata = ref reader.GetMetadata(index);

            width = metadata.Width;
            height = metadata.Height;
        }

        public byte[] ReadTexture(int index)
        {
            return reader.ReadArray(index);
        }

        public byte[] ReadTexture(int index, out ushort width, out ushort height)
        {
            byte[] data = reader.ReadArray(index, out TextureMetadata metadata);

            width = metadata.Width;
            height = metadata.Height;

            return data;
        }

        public int FillTexture(int index, Span<byte> data)
        {
            return reader.ReadSpan(index, data);
        }

        public void FillTexture(int index, Span<byte> data, out ushort width, out ushort height)
        {
            reader.ReadSpan(index, data, out TextureMetadata metadata);

            width = metadata.Width;
            height = metadata.Height;
        }

        public void Dispose()
        {
            reader.Dispose();
        }
    }
}
