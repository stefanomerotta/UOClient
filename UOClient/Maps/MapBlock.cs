using Microsoft.Extensions.ObjectPool;
using Microsoft.Xna.Framework.Graphics;
using System;
using UOClient.Data;
using UOClient.Maps.Components;
using UOClient.Maps.Statics;
using UOClient.Maps.Terrain;
using UOClient.Structures;
using UOClient.Utilities;

namespace UOClient.Maps
{
    internal sealed class MapBlock : IDisposable
    {
        public static readonly ObjectPool<MapBlock> Pool = Utility.CreatePool(16, new MapBlockPoolPolicy());

        private readonly TerrainBlock terrain;
        private readonly StaticsBlock statics;

        public MapBlock(GraphicsDevice device)
        {
            terrain = new(device);
            statics = new();
        }

        public void Initialize(GraphicsDevice device, int blockX, int blockY, MapFile map, StaticsDataFile staticsData)
        {
            int staticsCount = map.FillBlock(blockX, blockY, terrain.Tiles, statics.Tiles);

            terrain.Initialize(device, blockX, blockY);
            statics.Initialize(device, staticsData);
        }

        public void CleanUp()
        {
            terrain.CleanUp();
            statics.CleanUp();
        }

        public void Dispose()
        {
            terrain.Dispose();
            statics.Dispose();
        }

        private struct MapBlockPoolPolicy : IPooledObjectPolicy<MapBlock>
        {
            public MapBlock Create()
            {
                return new MapBlock(Globals.Device);
            }

            public bool Return(MapBlock obj)
            {
                obj.CleanUp();
                return true;
            }
        }
    }
}
