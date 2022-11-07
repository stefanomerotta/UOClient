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

        public bool TryLoad(int textureId, [NotNullWhen(true)] out byte[]? data)
        {
            data = null;

            SearchResult result = package.SearchExactFileName($"{dictionaryPrefix}{textureId:D8}.dds");
            if (!result.Found)
                return false;

            data = result.File.Unpack();
            return true;
        }
    }
}
