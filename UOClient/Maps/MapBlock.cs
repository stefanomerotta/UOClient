using Microsoft.Xna.Framework.Graphics;
using System;
using UOClient.Data;
using UOClient.Maps.Components;
using UOClient.Maps.Statics;
using UOClient.Maps.Terrain;
using UOClient.Utilities.SingleThreaded;

namespace UOClient.Maps
{
    internal sealed class MapBlock : IDisposable
    {
        public static readonly ObjectPool<MapBlock> Pool = new(25);

        public readonly TerrainBlock Terrain;
        public readonly StaticsBlock Statics;

        public int BlockX { get; private set; }
        public int BlockY { get; private set; }
        public bool Active { get; private set; }

        public MapBlock()
        {
            Terrain = new();
            Statics = new(null);
        }

        public void Initialize(int blockX, int blockY, TerrainFile map, StaticsFile statics, StaticData[] staticsData)
        {
            BlockX = blockX;
            BlockY = blockY;

            map.FillBlock(blockX, blockY, Terrain.Tiles);
            int staticsCount = statics.FillBlock(blockX, blockY, Statics.Tiles);

            Terrain.Initialize();
            Statics.Initialize(staticsData, staticsCount);

            Active = true;
        }

        public void CleanUp()
        {
            Active = false;

            BlockX = -1;
            BlockY = -1;
        }

        public void SendToVRAM(GraphicsDevice device)
        {
            Terrain.SendToVRAM(device);
            Statics.SendToVRAM(device);
        }

        public void ClearVRAM()
        {
            Terrain.ClearVRAM();
            Statics.ClearVRAM();
        }

        public void Dispose()
        {
            Terrain.Dispose();
            Statics.Dispose();
        }
    }
}
