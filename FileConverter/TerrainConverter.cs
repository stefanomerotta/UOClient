using Common.Utilities;
using FileSystem.Enums;
using FileSystem.IO;
using GameData.Structures.Contents.Terrains;

namespace FileConverter
{
    internal sealed class TerrainConverter: IDisposable
    {
        private readonly int newSize;
        private readonly int newChunkLength;

        private readonly int newChunkWidth;
        private readonly int newChunkHeight;
        private readonly CC.TerrainConverter terrainConverter;
        private readonly string outPath;

        public TerrainConverter(string inPath, string outPath, int id, int width, int height, int newChunkSize)
        {
            newSize = newChunkSize;
            newChunkLength = (newSize + 1) * (newSize + 1);

            newChunkWidth = (int)Math.Ceiling(width / (double)newSize);
            newChunkHeight = (int)Math.Ceiling(height / (double)newSize);

            terrainConverter = new(inPath, id, width, height, newSize);

            this.outPath = outPath;
        }

        public void Convert(string fileName)
        {
            using FileStream stream = File.Create(Path.Combine(outPath, fileName));
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

        public void Dispose()
        {
            terrainConverter.Dispose();
        }
    }
}
