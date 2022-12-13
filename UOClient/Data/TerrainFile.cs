using FileSystem.IO;
using GameData.Structures.Contents.Terrains;
using System;
using System.Diagnostics;
using System.IO;

namespace UOClient.Data
{
    internal sealed class TerrainFile : IDisposable
    {
        public const int BlockSize = 32;
        public const int BlockSizeShift = 5; // number of byteshift for converting between block and tile coordinates
        public const int BlockLength = (BlockSize + 1) * (BlockSize + 1);

        private readonly PackageReader reader;
        public readonly int BlocksWidth;
        public readonly int BlocksHeight;

        public TerrainFile(int width, int height)
        {
            FileStream stream = File.OpenRead(Path.Combine(Settings.FilePath, "terrain.bin"));
            reader = new(stream);

            BlocksWidth = (int)Math.Ceiling(width / (double)BlockSize);
            BlocksHeight = (int)Math.Ceiling(height / (double)BlockSize);
        }

        public unsafe void FillBlock(int blockX, int blockY, TerrainTile[] terrain)
        {
            Debug.Assert(blockX >= 0 && blockX < BlocksWidth);
            Debug.Assert(blockY >= 0 && blockY < BlocksHeight);
            Debug.Assert(terrain.Length == BlockLength);

            reader.ReadSpan(blockX + blockY * BlocksWidth, terrain.AsSpan());
        }

        public void Dispose()
        {
            reader.Dispose();
        }
    }
}
