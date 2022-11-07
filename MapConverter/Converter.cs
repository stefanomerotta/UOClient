using FileSystem.Enums;
using FileSystem.IO;
using FileConverter.Structures;
using System.Runtime.InteropServices;

namespace FileConverter
{
    internal class Converter
    {
        private const int newSize = 64;
        private static readonly List<StaticTile> EmptyStatics = new();

        private readonly int newChunkWidth;
        private readonly int newChunkHeight;
        private readonly CC.TerrainConverter terrainConverter;
        private readonly CC.StaticsConverter staticsConverter;

        public Converter(string path, int id, int width, int height)
        {
            newChunkWidth = (int)Math.Ceiling(width / (double)newSize);
            newChunkHeight = (int)Math.Ceiling(height / (double)newSize);

            terrainConverter = new(path, id, width, height);
            staticsConverter = new(path, id, width, height);
        }

        public unsafe void Convert(string fileName)
        {
            using FileStream stream = File.Create(Path.Combine("C:\\Program Files (x86)\\Electronic Arts\\Ultima Online Classic\\", fileName));
            using PackageWriter writer = new(stream);

            Span<TerrainTile> terrainChunk = new TerrainTile[(newSize + 1) * (newSize + 1)];
            Span<List<StaticTile>> staticsChunk = new List<StaticTile>[newSize * newSize];
            int terrainChunkByteLength = terrainChunk.Length * sizeof(TerrainTile);

            for (int y = 0; y < newChunkHeight; y++)
            {
                for (int x = 0; x < newChunkWidth; x++)
                {
                    terrainConverter.ConvertChunk(x, y, terrainChunk);
                    staticsConverter.ConvertChunk(x, y, staticsChunk);

                    int staticsLength = staticsChunk.Length;

                    for (int i = 0; i < staticsChunk.Length; i++)
                    {
                        if (staticsChunk[i] is null)
                            continue;

                        staticsLength += staticsChunk[i].Count * sizeof(StaticTile);
                    }

                    Span<byte> convertedChunk = new byte[terrainChunkByteLength + staticsLength];
                    MemoryMarshal.AsBytes(terrainChunk).CopyTo(convertedChunk);

                    Span<byte> convertedStaticChunks = convertedChunk[terrainChunkByteLength..];
                    int counter = 0;

                    for (int i = 0; i < staticsChunk.Length; i++)
                    {
                        List<StaticTile>? statics = staticsChunk[i] ?? EmptyStatics;
                        
                        convertedStaticChunks[counter++] = (byte)statics.Count;

                        Span<byte> rawStatics = MemoryMarshal.AsBytes(CollectionsMarshal.AsSpan(staticsChunk[i]));
                        rawStatics.CopyTo(convertedStaticChunks[counter..]);

                        counter += rawStatics.Length;
                    }

                    writer.WriteSpan(x + y * newChunkWidth, convertedChunk, CompressionAlgorithm.Zstd);

                    terrainChunk.Clear();

                    for (int i = 0; i < staticsChunk.Length; i++)
                    {
                        staticsChunk[i]?.Clear();
                    }
                }
            }
        }
    }
}
