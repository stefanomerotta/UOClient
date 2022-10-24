using FileSystem.Enums;
using FileSystem.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MapConverter
{
    public class Program
    {
        private const int oldSize = 8;
        private const int oldSizeOffset = 3;
        private const int newSize = 64;
        private const int newSizeOffset = 6;
        private const int deltaSizeOffset = newSizeOffset - oldSizeOffset;

        private readonly int width;
        private readonly int height;
        private readonly int oldChunkWidth;
        private readonly int oldChunkHeight;
        private readonly int newChunkWidth;
        private readonly int newChunkHeight;
        private readonly string path;
        private readonly UOPMap map;

        public Program(string path, int id, int width, int height)
        {
            this.path = path;
            this.width = width;
            this.height = height;

            oldChunkWidth = width >> 3; // width / 8
            oldChunkHeight = height >> 3; // width / 8

            newChunkWidth = (int)Math.Ceiling(width / (double)newSize);
            newChunkHeight = (int)Math.Ceiling(height / (double)newSize);

            map = new(path, id, width, height);
        }

        public static unsafe void Main(string[] args)
        {
            Program p = new(".\\", 4, 1440, 1440);
            p.Convert("converted2.bin");
        }

        public unsafe void Convert(string fileName)
        {
            using FileStream stream = File.Create(Path.Combine(path, fileName));
            using PackageWriter writer = new(stream);

            Span<MapTile> newChunk = new MapTile[(newSize + 1) * (newSize + 1)];

            for (int y = 0; y < newChunkHeight; y++)
            {
                for (int x = 0; x < newChunkWidth; x++)
                {
                    LoadOldChunks(x, y, newChunk);
                    LoadRightDelimiters(x, y, newChunk);
                    LoadBottomDelimiters(x, y, newChunk);
                    LoadCornerDelimiter(x, y, newChunk);

                    writer.WriteSpan(x + y * newChunkHeight, newChunk, CompressionAlgorithm.Zstd);

                    newChunk.Clear();
                }
            }
        }

        private void LoadOldChunk(int x, int y, Span<MapTile> tiles)
        {
            Debug.Assert(tiles.Length == oldSize * oldSize);

            if (x < oldChunkWidth && y < oldChunkHeight)
                map.FillChunk(x, y, tiles);
        }

        private void LoadOldChunks(int startX, int startY, Span<MapTile> tiles)
        {
            Debug.Assert(tiles.Length == (newSize + 1) * (newSize + 1));

            for (int x = 0; x < newSize >> deltaSizeOffset; x++)
            {
                int oldX = startX + x;

                for (int y = 0; y < newSize >> deltaSizeOffset; y++)
                {
                    int oldY = startY + y;

                    map.FillChunk(oldX, oldY, tiles.Slice(x + y * (newSize + 1), oldSize * oldSize));
                }
            }
        }

        private unsafe void LoadRightDelimiters(int startX, int startY, Span<MapTile> tiles)
        {
            Debug.Assert(tiles.Length == (newSize + 1) * (newSize + 1));

            if (startX >= newChunkWidth - 1)
                return;

            int oldX = startX + (newSize >> deltaSizeOffset) + 1;
            const int tileX = newSize;

            Span<byte> rawOldChunk = stackalloc byte[(oldSize * oldSize) * sizeof(MapTile)];
            Span<MapTile> oldChunk = MemoryMarshal.Cast<byte, MapTile>(rawOldChunk);

            for (int y = 0; y < newSize >> deltaSizeOffset; y++)
            {
                int oldY = startY + y;
                int tileY = y << deltaSizeOffset;

                map.FillChunk(oldX, oldY, oldChunk);

                for (int k = 0; k < oldSize; k++)
                {
                    tiles[tileX + (tileY + k) * (newSize + 1)] = oldChunk[k * oldSize];
                }
            }
        }

        private unsafe void LoadBottomDelimiters(int startX, int startY, Span<MapTile> tiles)
        {
            Debug.Assert(tiles.Length == (newSize + 1) * (newSize + 1));

            if (startY >= newChunkHeight - 1)
                return;

            int oldY = startY + (newSize >> deltaSizeOffset) + 1;
            const int tileY = newSize;

            Span<byte> rawOldChunk = stackalloc byte[(oldSize * oldSize) * sizeof(MapTile)];
            Span<MapTile> oldChunk = MemoryMarshal.Cast<byte, MapTile>(rawOldChunk);

            for (int x = 0; x < newSize >> deltaSizeOffset; x++)
            {
                int oldX = startX + x;
                int tileX = x << deltaSizeOffset;

                map.FillChunk(oldX, oldY, oldChunk);

                for (int k = 0; k < oldSize; k++)
                {
                    tiles[tileX + k + tileY * (newSize + 1)] = oldChunk[k];
                }
            }
        }

        private unsafe void LoadCornerDelimiter(int startX, int startY, Span<MapTile> tiles)
        {
            Debug.Assert(tiles.Length == (newSize + 1) * (newSize + 1));

            if (startY >= newChunkWidth - 1 && startY >= newChunkHeight - 1)
                return;

            int oldX = startX + (newSize >> deltaSizeOffset) + 1;
            int oldY = startY + (newSize >> deltaSizeOffset) + 1;

            const int tileX = newSize;
            const int tileY = newSize;

            Span<byte> rawOldChunk = stackalloc byte[(oldSize * oldSize) * sizeof(MapTile)];
            Span<MapTile> oldChunk = MemoryMarshal.Cast<byte, MapTile>(rawOldChunk);

            map.FillChunk(oldX, oldY, oldChunk);

            tiles[tileX + tileY * (newSize + 1)] = oldChunk[0];
        }
    }
}