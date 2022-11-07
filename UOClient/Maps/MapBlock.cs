using System;
using UOClient.Data;
using UOClient.Structures;

namespace UOClient.Maps
{
    internal sealed class MapBlock : IDisposable
    {
        public const int Size = Map.BlockSize;
        public const int SizeShift = Map.BlockSizeShift;

        private readonly TerrainTile[] terrain;

        public MapBlock(int blockX, int blockY, Map map)
        {
            map.GetBlock(blockX, blockY, null);
        }

        public void Dispose()
        {
        }
    }
}
