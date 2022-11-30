using Common.Utilities;
using FileConverter.Structures;
using FileSystem.Enums;
using FileSystem.IO;

namespace FileConverter
{
    internal class TerrainConverter
    {
        private const int newSize = 64;
        private const int newChunkLength = (newSize + 1) * (newSize + 1);

        private readonly int newChunkWidth;
        private readonly int newChunkHeight;
        private readonly CC.TerrainConverter terrainConverter;

        public TerrainConverter(string path, int id, int width, int height)
        {
            newChunkWidth = (int)Math.Ceiling(width / (double)newSize);
            newChunkHeight = (int)Math.Ceiling(height / (double)newSize);

            terrainConverter = new(path, id, width, height);
        }

        public void Convert(string fileName)
        {
            using FileStream stream = File.Create(Path.Combine("C:\\Program Files (x86)\\Electronic Arts\\Ultima Online Classic\\", fileName));
            using PackageWriter writer = new(stream);

            TerrainTile[] chunk = new TerrainTile[newChunkLength];

            for (int y = 0; y < newChunkHeight; y++)
            {
                for (int x = 0; x < newChunkWidth; x++)
                {
                    terrainConverter.ConvertChunk(x, y, chunk);
                    writer.WriteSpan(x + y * newChunkWidth, chunk.AsReadOnlySpan(), CompressionAlgorithm.Zstd);
                }
            }
        }
    }
}
