using FileConverter.Utilities;
using MYPReader;
using System.Diagnostics.CodeAnalysis;

namespace FileConverter.EC
{
    internal sealed class TextureLoader : IDisposable
    {
        private readonly MythicPackage package;
        private readonly string dictionaryPrefix;
        private byte[] buffer;

        public TextureLoader(string filePath, string dictionaryPrefix)
        {
            package = new(filePath);
            this.dictionaryPrefix = dictionaryPrefix;
            buffer = Array.Empty<byte>();
        }

        public bool TryLoad(int textureId, [NotNullWhen(true)] out ReadOnlySpan<byte> data, out int width, out int height)
        {
            data = null;
            width = 0;
            height = 0;

            int byteRead = package.UnpackFile($"{dictionaryPrefix}{textureId:D8}.dds", ref buffer);
            if (byteRead == 0)
                return false;

            data = DDSReader.GetContent(buffer.AsSpan(0, byteRead), out width, out height);

            return true;
        }

        public void Dispose()
        {
            package.Dispose();
        }
    }
}
