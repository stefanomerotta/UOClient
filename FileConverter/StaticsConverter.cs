using FileConverter.Structures;
using FileSystem.Enums;
using FileSystem.IO;
using GameData.Structures.Headers;
using System.Runtime.InteropServices;

namespace FileConverter
{
    internal sealed class StaticsConverter : IDisposable
    {
        private readonly int newSize;
        private readonly int newChunkLength;

        private readonly int newChunkWidth;
        private readonly int newChunkHeight;
        private readonly CC.StaticsConverter staticsConverter;
        private readonly string outPath;

        public StaticsConverter(string inPath, string outPath, int id, int width, int height, int newChunkSize)
        {
            newSize = newChunkSize;
            newChunkLength = newSize * newSize;

            newChunkWidth = (int)Math.Ceiling(width / (double)newSize);
            newChunkHeight = (int)Math.Ceiling(height / (double)newSize);

            staticsConverter = new(inPath, id, width, height, newSize);
            
            this.outPath = outPath;
        }

        public unsafe void Convert(string fileName)
        {
            using FileStream stream = File.Create(Path.Combine(outPath, fileName));
            using PackageWriter<StaticsMetadata> writer = new(stream);

            Span<List<StaticTile>> staticsChunk = new List<StaticTile>[newChunkLength];
            Span<byte> chunk = Span<byte>.Empty;

            for (int y = 0; y < newChunkHeight; y++)
            {
                for (int x = 0; x < newChunkWidth; x++)
                {
                    int count = staticsConverter.ConvertChunk(x, y, staticsChunk);
                    if (count == 0)
                        continue;

                    if (chunk.Length < count * sizeof(StaticTile) + newChunkLength)
                        chunk = new byte[count * sizeof(StaticTile) + newChunkLength];

                    int index = 0;
                    for (int k = 0; k < newChunkLength; k++)
                    {
                        List<StaticTile> list = staticsChunk[k];

                        if (list is not { Count: > 0 })
                        {
                            chunk[index++] = 0;
                            continue;
                        }
                        
                        chunk[index++] = (byte)list.Count;

                        list.Sort(default(ZComparer));

                        MemoryMarshal.AsBytes(CollectionsMarshal.AsSpan(list)).CopyTo(chunk[index..]);
                        index += list.Count * sizeof(StaticTile);

                        list.Clear();
                    }

                    writer.WriteSpan(x + y * newChunkWidth, chunk[..index], CompressionAlgorithm.Zstd, new(count));
                }
            }
        }

        public void Dispose()
        {
            staticsConverter.Dispose();
        }

        private struct ZComparer : IComparer<StaticTile>
        {
            public int Compare(StaticTile x, StaticTile y)
            {
                if (x.Z < y.Z)
                    return -1;

                if (x.Z == y.Z)
                    return 0;

                return 1;
            }
        }
    }
}
