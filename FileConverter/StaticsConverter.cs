using FileConverter.Structures;
using FileSystem.Enums;
using FileSystem.IO;
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

        public StaticsConverter(string path, int id, int width, int height, int newChunkSize)
        {
            newSize = newChunkSize;
            newChunkLength = newSize * newSize;

            newChunkWidth = (int)Math.Ceiling(width / (double)newSize);
            newChunkHeight = (int)Math.Ceiling(height / (double)newSize);

            staticsConverter = new(path, id, width, height, newSize);
        }

        public unsafe void Convert(string fileName)
        {
            using FileStream stream = File.Create(Path.Combine("C:\\Program Files (x86)\\Electronic Arts\\Ultima Online Classic\\", fileName));
            using PackageWriter writer = new(stream);

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

                    writer.WriteSpan(x + y * newChunkWidth, chunk[..index], CompressionAlgorithm.Zstd);
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
