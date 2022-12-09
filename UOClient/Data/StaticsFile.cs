using FileSystem.IO;
using GameData.Structures.Headers;
using System;
using System.Diagnostics;
using System.IO;
using UOClient.Maps.Components;

namespace UOClient.Data
{
    internal sealed class StaticsFile : IDisposable
    {
        public const int BlockSize = TerrainFile.BlockSize;
        public const int BlockSizeShift = TerrainFile.BlockSizeShift; // number of byteshift for converting between block and tile coordinates
        public const int BlockLength = BlockSize * BlockSize;

        private readonly PackageReader<StaticsMetadata> reader;
        public readonly int BlocksWidth;
        public readonly int BlocksHeight;

        public StaticsFile(int width, int height)
        {
            FileStream stream = File.Open(Path.Combine(Settings.FilePath, "statics.bin"), FileMode.Open);
            reader = new(stream);

            BlocksWidth = (int)Math.Ceiling(width / (double)BlockSize);
            BlocksHeight = (int)Math.Ceiling(height / (double)BlockSize);
        }

        public unsafe StaticTileList FillBlock(int blockX, int blockY)
        {
            Debug.Assert(blockX >= 0 && blockX < BlocksWidth);
            Debug.Assert(blockY >= 0 && blockY < BlocksHeight);

            byte[] block = reader.ReadArray(blockX + blockY * BlocksWidth, out StaticsMetadata metadata);
            return new StaticTileList(block, metadata.TotalStaticsCount);
        }

        public void Dispose()
        {
            reader.Dispose();
        }
    }
}
