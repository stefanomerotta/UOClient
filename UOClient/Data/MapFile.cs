using FileSystem.IO;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using UOClient.Structures;
using UOClient.Utilities;

namespace UOClient.Data
{
    internal sealed class Map : IDisposable
    {
        public const int BlockSize = 64;
        public const int BlockSizeShift = 6; // number of byteshift for converting between block and tile coordinates
        private const int terrainBlockLength = (BlockSize + 1) * (BlockSize + 1);
        private const int staticsBlockLength = BlockSize * BlockSize;

        private static readonly int terrainBlockByteLength = terrainBlockLength * Unsafe.SizeOf<TerrainTile>();

        private readonly PackageReader reader;
        private readonly int blocksWidth;
        private readonly int blocksHeight;

        public Map(int width, int height)
        {
            FileStream stream = File.Open(Path.Combine(Settings.FilePath, "converted.bin"), FileMode.Open);
            reader = new(stream);

            blocksWidth = (int)Math.Ceiling(width / (double)BlockSize);
            blocksHeight = (int)Math.Ceiling(height / (double)BlockSize);
        }

        public unsafe Span<StaticTile[]> GetBlock(int blockX, int blockY, TerrainTile[] terrain)
        {
            Debug.Assert(terrain.Length == terrainBlockLength);
            Debug.Assert(blockX >= 0 && blockX < blocksWidth);
            Debug.Assert(blockY >= 0 && blockY < blocksHeight);

            Span<byte> block = reader.ReadSpan(blockX + blockY * blocksWidth);

            block[..terrainBlockByteLength].Cast<byte, TerrainTile>().CopyTo(terrain);

            Span<byte> staticsBlock = block[terrainBlockByteLength..];
            Span<StaticTile[]> statics = new StaticTile[staticsBlockLength][];
            
            int counter = 0;
            for (int i = 0; i < staticsBlockLength; i++)
            {
                int count = staticsBlock[counter++];

                if (count == 0)
                {
                    statics[i] = Array.Empty<StaticTile>();
                    continue;
                }

                int byteCount = count * sizeof(StaticTile);

                statics[i] = staticsBlock.Slice(counter, byteCount).Cast<byte, StaticTile>().ToArray();
                counter += byteCount;
            }

            return statics;
        }

        public void Dispose()
        {
            reader.Dispose();
        }
    }
}
