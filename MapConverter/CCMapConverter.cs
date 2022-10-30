using FileSystem.Enums;
using FileSystem.IO;
using MapConverter.UOP;
using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

namespace MapConverter
{
    internal class CCMapConverter
    {
        private const int oldSize = 8;
        private const int oldSizeOffset = 3;
        private const int newSize = 64;
        private const int newSizeOffset = 6;
        private const int deltaSizeOffset = newSizeOffset - oldSizeOffset;
        private const int deltaSize = newSize >> deltaSizeOffset;

        private readonly int width;
        private readonly int height;
        private readonly int oldChunkWidth;
        private readonly int oldChunkHeight;
        private readonly int newChunkWidth;
        private readonly int newChunkHeight;
        private readonly string path;
        private readonly UOPMap map;

        public CCMapConverter(string path, int id, int width, int height)
        {
            this.path = path;
            this.width = width;
            this.height = height;

            oldChunkWidth = width >> oldSizeOffset;
            oldChunkHeight = height >> oldSizeOffset;

            newChunkWidth = (int)Math.Ceiling(width / (double)newSize);
            newChunkHeight = (int)Math.Ceiling(height / (double)newSize);

            map = new(path, id, width, height);
        }

        public unsafe void Convert(string fileName)
        {
            using FileStream stream = File.Create(Path.Combine("C:\\Program Files (x86)\\Electronic Arts\\Ultima Online Classic\\", fileName));
            using PackageWriter writer = new(stream);

            Span<MapTile> newChunk = new MapTile[(newSize + 1) * (newSize + 1)];

            for (int y = 0; y < newChunkHeight; y++)
            {
                for (int x = 0; x < newChunkWidth; x++)
                {
                    int oldX = x << deltaSizeOffset;
                    int oldY = y << deltaSizeOffset;

                    LoadOldChunks(oldX, oldY, newChunk);
                    LoadRightDelimiters(oldX, oldY, newChunk);
                    LoadBottomDelimiters(oldX, oldY, newChunk);
                    LoadCornerDelimiter(oldX, oldY, newChunk);

                    writer.WriteSpan(x + y * newChunkHeight, newChunk, CompressionAlgorithm.Zstd);

                    newChunk.Clear();
                }
            }
        }

        private void LoadOldChunk(int x, int y, Span<MapTile> tiles)
        {
            Debug.Assert(tiles.Length == oldSize * oldSize);

            if (x >= oldChunkWidth || y >= oldChunkHeight)
                return;

            map.FillChunk(x, y, tiles);

            for (int i = 0; i < tiles.Length; i++)
            {
                ref MapTile tile = ref tiles[i];
                tile.Id = (ushort)MapTileTranscoder.GetNewId(tile.Id);
            }
        }

        private unsafe void LoadOldChunks(int startX, int startY, Span<MapTile> tiles)
        {
            Debug.Assert(tiles.Length == (newSize + 1) * (newSize + 1));

            Span<byte> rawOldChunk = stackalloc byte[oldSize * oldSize * sizeof(MapTile)];
            Span<MapTile> oldChunk = MemoryMarshal.Cast<byte, MapTile>(rawOldChunk);

            for (int x = 0; x < deltaSize; x++)
            {
                int oldX = startX + x;
                int tileX = x << deltaSizeOffset;

                for (int y = 0; y < deltaSize; y++)
                {
                    int oldY = startY + y;
                    int tileY = y << deltaSizeOffset;

                    LoadOldChunk(oldX, oldY, oldChunk);

                    for (int k = 0; k < oldSize; k++)
                    {
                        oldChunk.Slice(k * oldSize, oldSize).CopyTo(tiles.Slice(tileX + (tileY + k) * (newSize + 1), oldSize));
                    }
                }
            }
        }

        private unsafe void LoadRightDelimiters(int startX, int startY, Span<MapTile> tiles)
        {
            Debug.Assert(tiles.Length == (newSize + 1) * (newSize + 1));

            if (startX >= oldChunkWidth - 1)
                return;

            int oldX = startX + deltaSize;
            const int tileX = newSize;

            Span<byte> rawOldChunk = stackalloc byte[oldSize * oldSize * sizeof(MapTile)];
            Span<MapTile> oldChunk = MemoryMarshal.Cast<byte, MapTile>(rawOldChunk);

            for (int y = 0; y < deltaSize; y++)
            {
                int oldY = startY + y;
                int tileY = y << deltaSizeOffset;

                LoadOldChunk(oldX, oldY, oldChunk);

                for (int k = 0; k < oldSize; k++)
                {
                    tiles[tileX + (tileY + k) * (newSize + 1)] = oldChunk[k * oldSize];
                }
            }
        }

        private unsafe void LoadBottomDelimiters(int startX, int startY, Span<MapTile> tiles)
        {
            Debug.Assert(tiles.Length == (newSize + 1) * (newSize + 1));

            if (startY >= oldChunkHeight - 1)
                return;

            int oldY = startY + deltaSize;
            const int tileY = newSize;

            Span<byte> rawOldChunk = stackalloc byte[oldSize * oldSize * sizeof(MapTile)];
            Span<MapTile> oldChunk = MemoryMarshal.Cast<byte, MapTile>(rawOldChunk);

            for (int x = 0; x < deltaSize; x++)
            {
                int oldX = startX + x;
                int tileX = x << deltaSizeOffset;

                LoadOldChunk(oldX, oldY, oldChunk);

                for (int k = 0; k < oldSize; k++)
                {
                    tiles[tileX + k + tileY * (newSize + 1)] = oldChunk[k];
                }
            }
        }

        private unsafe void LoadCornerDelimiter(int startX, int startY, Span<MapTile> tiles)
        {
            Debug.Assert(tiles.Length == (newSize + 1) * (newSize + 1));

            if (startX >= oldChunkWidth - 1 && startY >= oldChunkHeight - 1)
                return;

            int oldX = startX + deltaSize;
            int oldY = startY + deltaSize;

            const int tileX = newSize;
            const int tileY = newSize;

            Span<byte> rawOldChunk = stackalloc byte[oldSize * oldSize * sizeof(MapTile)];
            Span<MapTile> oldChunk = MemoryMarshal.Cast<byte, MapTile>(rawOldChunk);

            LoadOldChunk(oldX, oldY, oldChunk);

            tiles[tileX + tileY * (newSize + 1)] = oldChunk[0];
        }
    }
}
