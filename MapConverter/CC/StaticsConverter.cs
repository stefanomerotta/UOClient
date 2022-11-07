using FileConverter.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using NewStaticTile = FileConverter.Structures.StaticTile;

namespace FileConverter.CC
{
    internal class StaticsConverter
    {
        private const int oldSizeShift = 3;
        private const int newSize = 64;
        private const int newSizeShift = 6;
        private const int deltaSizeShift = newSizeShift - oldSizeShift;
        private const int deltaSize = newSize >> deltaSizeShift;

        private readonly FileReader idxReader;
        private readonly FileReader staticsReader;

        private readonly int oldChunkWidth;
        private readonly int oldChunkHeight;
        private readonly int newChunkWidth;

        public StaticsConverter(string path, int id, int width, int height)
        {
            oldChunkWidth = width >> oldSizeShift;
            oldChunkHeight = height >> oldSizeShift;

            newChunkWidth = (int)Math.Ceiling(width / (double)newSize);

            idxReader = new(Path.Combine(path, $"staidx{id}.mul"));
            staticsReader = new(Path.Combine(path, $"statics{id}.mul"));
        }

        public unsafe void ConvertChunk(int startX, int startY, Span<List<NewStaticTile>> tiles)
        {
            Debug.Assert(tiles.Length == newSize * newSize);

            for (int x = 0; x < deltaSize; x++)
            {
                int oldX = startX + x;

                for (int y = 0; y < deltaSize; y++)
                {
                    int oldY = startY + y;
                    LoadOldChunk(oldX, oldY, tiles);
                }
            }
        }

        private unsafe void LoadOldChunk(int oldX, int oldY, Span<List<NewStaticTile>> tiles)
        {
            if (oldX >= oldChunkWidth || oldY >= oldChunkHeight)
                return;

            int startX = oldX >> deltaSizeShift;
            int startY = oldY >> deltaSizeShift;

            idxReader.Seek((oldX * oldChunkHeight + oldY) * 12);

            int offset = idxReader.ReadInt32();
            int length = idxReader.ReadInt32();

            if (offset < 0 || length <= 0)
                return;

            Span<byte> rawOldChunk = stackalloc byte[length];
            Span<StaticTile> oldChunk = MemoryMarshal.Cast<byte, StaticTile>(rawOldChunk);

            staticsReader.Seek(offset);
            staticsReader.ReadSpan(oldChunk);

            for (int i = 0; i < oldChunk.Length; i++)
            {
                ref StaticTile oldTile = ref oldChunk[i];
                ref List<NewStaticTile> list = ref tiles[startX + oldTile.X + (startY + oldTile.Y) * newChunkWidth];

                list ??= new(4);

                list.Add(new()
                {
                    Id = oldTile.Id,
                    Z = oldTile.Z,
                    Color = oldTile.Color
                });
            }
        }
    }
}
