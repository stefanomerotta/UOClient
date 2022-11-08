using Microsoft.Extensions.ObjectPool;
using Microsoft.Xna.Framework.Graphics;
using System;
using UOClient.Data;
using UOClient.Maps.Components;
using UOClient.Maps.Statics;
using UOClient.Maps.Terrain;
using UOClient.Utilities;

namespace UOClient.Maps
{
    internal sealed class MapBlock : IDisposable
    {
        public static readonly ObjectPool<MapBlock> Pool = Utility.CreatePool(16, new MapBlockPoolPolicy());

        public readonly TerrainBlock Terrain;
        public readonly StaticsBlock Statics;

        private MapBlock(GraphicsDevice device)
        {
            Terrain = new(device);
            Statics = new();
        }

        public void Initialize(GraphicsDevice device, int blockX, int blockY, MapFile map, StaticData[] staticsData)
        {
            int staticsCount = map.FillBlock(blockX, blockY, Terrain.Tiles, Statics.Tiles);
            Terrain.Initialize(device, blockX, blockY);
            Statics.Initialize(device, blockX, blockY, staticsData, staticsCount);
        }

        public void CleanUp()
        {
            Terrain.CleanUp();
            Statics.CleanUp();
        }

        public void Dispose()
        {
            Terrain.Dispose();
            Statics.Dispose();
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
