using FileConverter.Utilities;
using Mythic.Package;
using System.Diagnostics.CodeAnalysis;

namespace FileConverter.EC
{
    internal class TextureLoader
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

            SearchResult result = package.SearchExactFileName($"{dictionaryPrefix}{textureId:D8}.dds");
            if (!result.Found)
                return false;

            byte[] unpacked = result.File.Unpack();
            data = DDSReader.GetContent(unpacked, out width, out height);

            return true;
        }
    }
}
