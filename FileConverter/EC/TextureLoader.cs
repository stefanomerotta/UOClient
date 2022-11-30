using FileConverter.Utilities;
using MYPReader;
using System.Diagnostics.CodeAnalysis;

namespace FileConverter.EC
{
    internal sealed class TextureLoader : IDisposable
    {
        private readonly MythicPackage package;
        private readonly string dictionaryPrefix;

        public TextureLoader(string filePath, string dictionaryPrefix)
        {
            package = new(filePath);
            this.dictionaryPrefix = dictionaryPrefix;
        }

        public bool TryLoad(int textureId, [NotNullWhen(true)] out Span<byte> data, out int width, out int height)
        {
            data = null;
            width = 0;
            height = 0;

            byte[] bytes = package.UnpackFile($"{dictionaryPrefix}{textureId:D8}.dds");
            if (bytes.Length == 0)
                return false;

            data = DDSReader.GetContent(bytes, out width, out height);

            return true;
        }

        public void Dispose()
        {
            package.Dispose();
        }
    }
}
