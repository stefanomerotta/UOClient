﻿using FileConverter.Structures;
using FileSystem.Enums;
using FileSystem.IO;
using System.Runtime.InteropServices;

namespace FileConverter
{
    internal class StaticsConverter
    {
        private const int newSize = 64;
        private const int newChunkLength = newSize * newSize;

        private readonly int newChunkWidth;
        private readonly int newChunkHeight;
        private readonly CC.StaticsConverter staticsConverter;

        public StaticsConverter(string path, int id, int width, int height)
        {
            newChunkWidth = (int)Math.Ceiling(width / (double)newSize);
            newChunkHeight = (int)Math.Ceiling(height / (double)newSize);

            staticsConverter = new(path, id, width, height);
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

                        MemoryMarshal.AsBytes(CollectionsMarshal.AsSpan(list)).CopyTo(chunk[index..]);
                        index += list.Count * sizeof(StaticTile);

                        list.Clear();
                    }

                    writer.WriteSpan(x + y * newChunkWidth, chunk[..index], CompressionAlgorithm.Zstd);
                }
            }
        }
    }
}
