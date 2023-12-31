﻿using FileConverter.CC.Structures;
using FileConverter.IO;
using System.Diagnostics;
using NewStaticTile = GameData.Structures.Contents.Statics.StaticTile;

namespace FileConverter.CC
{
    internal sealed class StaticsConverter : IDisposable
    {
        private const int oldSizeShift = 3;
        private const int oldSize = 8;

        private readonly int newSize;
        private readonly int newSizeShift;
        private readonly int deltaSizeShift;
        private readonly int deltaSize;

        private readonly FileReader idxReader;
        private readonly FileReader staticsReader;

        private readonly int oldChunkWidth;
        private readonly int oldChunkHeight;
        private readonly int newChunkWidth;

        public StaticsConverter(string path, int id, int width, int height, int newChunkSize)
        {
            oldChunkWidth = width >> oldSizeShift;
            oldChunkHeight = height >> oldSizeShift;

            newSize = newChunkSize;
            newSizeShift = (int)Math.Log2(newSize);
            deltaSize = newSize / oldSize;
            deltaSizeShift = (int)Math.Log2(deltaSize);

            newChunkWidth = (int)Math.Ceiling(width / (double)newSize);

            idxReader = new(Path.Combine(path, $"staidx{id}.mul"));
            staticsReader = new(Path.Combine(path, $"statics{id}.mul"));
        }

        public int ConvertChunk(int startX, int startY, Span<List<NewStaticTile>> tiles)
        {
            Debug.Assert(tiles.Length == newSize * newSize);

            startX <<= deltaSizeShift;
            startY <<= deltaSizeShift;

            int totalCount = 0;

            for (int x = 0; x < deltaSize; x++)
            {
                int oldX = startX + x;

                for (int y = 0; y < deltaSize; y++)
                {
                    int oldY = startY + y;
                    totalCount += LoadOldChunk(oldX, oldY, tiles);
                }
            }

            return totalCount;
        }

        public void Dispose()
        {
            idxReader.Dispose();
            staticsReader.Dispose();
        }

        private unsafe int LoadOldChunk(int oldX, int oldY, Span<List<NewStaticTile>> tiles)
        {
            if (oldX >= oldChunkWidth || oldY >= oldChunkHeight)
                return 0;

            int startX = oldX % deltaSize * oldSize;
            int startY = oldY % deltaSize * oldSize;

            idxReader.Seek((oldX * oldChunkHeight + oldY) * 12);

            int offset = idxReader.ReadInt32();
            int length = idxReader.ReadInt32();

            if (offset < 0 || length <= 0)
                return 0;

            int count = 0;
            Span<StaticTile> oldChunk = stackalloc StaticTile[length / sizeof(StaticTile)];

            staticsReader.Seek(offset);
            staticsReader.ReadSpan(oldChunk);

            for (int i = 0; i < oldChunk.Length; i++)
            {
                ref StaticTile oldTile = ref oldChunk[i];
                ref List<NewStaticTile> list = ref tiles[startX + oldTile.X + (startY + oldTile.Y) * newSize];

                list ??= new(4);
                list.Add(new(oldTile.Id, oldTile.Color, oldTile.Z));

                count++;
            }

            return count;
        }
    }
}
