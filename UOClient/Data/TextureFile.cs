using FileSystem.IO;
using GameData.Structures.Headers;
using System;
using System.IO;

namespace UOClient.Data
{
    internal sealed class TextureFile
    {
        private readonly PackageReader<TextureMetadata> reader;

        public TextureFile(string fileName)
        {
            FileStream stream = File.Open(Path.Combine(Settings.FilePath, fileName), FileMode.Open);
            reader = new(stream);
        }

        public byte[] ReadTexture(int index, out int width, out int height)
        {
            byte[] data = reader.ReadArray(index, out TextureMetadata metadata);

            width = metadata.Width;
            height = metadata.Height;

            return data;
        }

        public void FillTexture(int index, Span<byte> data, out int width, out int height)
        {
             reader.ReadSpan(index, data);

            if (BitConverter.ToInt32(data) != 0x20534444)
                throw new Exception();

            width = BitConverter.ToInt32(data[9..]);
            height = BitConverter.ToInt32(data[11..]);
        }
    }
}
