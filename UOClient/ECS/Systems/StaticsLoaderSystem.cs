using DefaultEcs;
using DefaultEcs.System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Threading;
using System.Threading.Tasks;
using UOClient.Data;
using UOClient.ECS.Components;
using UOClient.ECS.Events;
using UOClient.Maps.Components;
using UOClient.Maps.Statics;
using UOClient.Maps.Terrain;
using UOClient.Utilities;
using UOClient.Utilities.SingleThreaded;

namespace UOClient.ECS.Systems
{
    internal class StaticsLoaderSystem : ISystem<GameTime>
    {
        private const int poolSize = 25;

        private static readonly ObjectPool<StaticsBlock> pool = new(poolSize);

        private readonly GraphicsDevice device;
        private readonly World world;
        private readonly StaticsFile staticsFile;
        private readonly TextureFile textureFile;
        private readonly StaticData[] staticsData;
        private readonly EntityMap<Sector> statics;
        
        private readonly AsyncCommandProcessor<StaticsBlock> staticsToLoad;
        private readonly CommandQueue<StaticsBlock> staticsToSync;
        private readonly CancellationTokenSource source;

        private readonly IDisposable sectorAddedSubscription;
        private readonly IDisposable sectorRemovedSubscription;

        public bool IsEnabled { get; set; }

        public StaticsLoaderSystem(World world, GraphicsDevice device, StaticsFile staticsFile, 
            TextureFile textureFile, StaticData[] staticsData)
        {
            this.world = world;
            this.device = device;
            this.staticsFile = staticsFile;
            this.textureFile = textureFile;
            this.staticsData = staticsData;

            source = new();

            staticsToLoad = new(poolSize, LoadBlock, source.Token);
            staticsToSync = new(poolSize);

            statics = world.GetEntities()
                .With<StaticsBlock>()
                .Without<TerrainBlock>()
                .AsMap<Sector>();

            sectorAddedSubscription = world.Subscribe<SectorAdded>(OnSectorAdded);
            sectorRemovedSubscription = world.Subscribe<SectorRemoved>(OnSectorRemoved);
        }

        public void Update(GameTime state)
        {
            while (staticsToSync.TryDequeue(out StaticsBlock? block))
            {
                Entity e = world.CreateEntity();

                e.Set(new Sector(block.X, block.Y));
                e.Set(block);

                if (block.TotalStaticsCount == 0)
                    continue;

                block.SendToVRAM(device);

                //StaticTile[][] tiles = block.Tiles;

                //for (int i = 0; i < tiles.Length; i++)
                //{
                //    StaticTile[] tileStatics = tiles[i];

                //    if (tileStatics.Length == 0)
                //        continue;

                //    int x = i % TerrainFile.BlockSize;
                //    int y = i / TerrainFile.BlockSize;

                //    for (int j = 0; j < tileStatics.Length; j++)
                //    {
                //        ref StaticTile tile = ref tileStatics[j];

                //        Entity @static = world.CreateEntity();

                //        @static.Set(new Position(x, y, tile.Z));
                //        @static.Set(new Sector(block.X, block.Y));
                //        @static.Set(new StaticTexture(tile.Id));
                //    }
                //}
            }
        }

        private void OnSectorAdded(in SectorAdded sector)
        {
            if (!pool.TryGet(out StaticsBlock? block))
                block = new(textureFile);

            block.X = sector.X;
            block.Y = sector.Y;

            staticsToLoad.TryEnqueue(block);
        }

        private void OnSectorRemoved(in SectorRemoved sector)
        {
            if (!statics.TryGetEntity(UnsafeUtility.As<SectorRemoved, Sector>(sector), out Entity e))
                return;

            StaticsBlock block = e.Get<StaticsBlock>();
            
            block.ClearVRAM();
            pool.Return(block);

            e.Dispose();
        }

        private ValueTask LoadBlock(StaticsBlock block)
        {
            int count = staticsFile.FillBlock(block.X, block.Y, block.Tiles);
            block.Initialize(staticsData, count);

            return staticsToSync.EnqueueAsync(block);
        }

        public void Dispose()
        {
            sectorAddedSubscription.Dispose();
            sectorRemovedSubscription.Dispose();

            source.Cancel();

#pragma warning disable CA2012

            if (staticsToLoad.DisposeAsync() is { IsCompleted: false } task)
                task.GetAwaiter().GetResult();

#pragma warning restore CA2012
        }
    }
}
