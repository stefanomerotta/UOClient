using DefaultEcs;
using DefaultEcs.System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Threading;
using System.Threading.Tasks;
using UOClient.Data;
using UOClient.ECS.Components;
using UOClient.Maps.Components;
using UOClient.Maps.Statics;
using UOClient.Utilities;
using UOClient.Utilities.SingleThreaded;

namespace UOClient.ECS.Systems
{
    internal class StaticsLoaderSystem : ISystem<GameTime>
    {
        private const int poolSize = 25;

        private static readonly ObjectPool<StaticsBlock> pool = new(poolSize);

        private readonly GraphicsDevice device;
        private readonly StaticsFile staticsFile;
        private readonly TextureFile textureFile;
        private readonly StaticData[] staticsData;
        private readonly EntityMap<Block> blocks;

        private readonly AsyncCommandProcessor<StaticsBlock> staticsToLoad;
        private readonly CommandQueue<StaticsBlock> staticsToSync;
        private readonly CancellationTokenSource source;

        public bool IsEnabled { get; set; }

        public StaticsLoaderSystem(World world, GraphicsDevice device, StaticsFile staticsFile,
            TextureFile textureFile, StaticData[] staticsData)
        {
            this.device = device;
            this.staticsFile = staticsFile;
            this.textureFile = textureFile;
            this.staticsData = staticsData;

            source = new();

            staticsToLoad = new(poolSize, LoadBlock, source.Token);
            staticsToSync = new(poolSize);

            blocks = world.GetEntities().AsMap<Block>();

            blocks.EntityAdded += OnBlockAdded;
            blocks.EntityRemoved += OnBlockRemoved;
        }

        public void Update(GameTime state)
        {
            while (staticsToSync.TryDequeue(out StaticsBlock? block))
            {
                if (!blocks.TryGetEntity(new(block.X, block.Y), out Entity e))
                {
                    pool.Return(block);
                    continue;
                }

                e.Set(block);
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

        private void OnBlockAdded(in Entity e)
        {
            if (!pool.TryGet(out StaticsBlock? block))
                block = new(textureFile);

            Block blockComponent = e.Get<Block>();

            block.X = blockComponent.X;
            block.Y = blockComponent.Y;

            staticsToLoad.TryEnqueue(block);
        }

        private void OnBlockRemoved(in Entity e)
        {
            if (!e.Has<StaticsBlock>())
                return;

            StaticsBlock block = e.Get<StaticsBlock>();

            block.ClearVRAM();
            pool.Return(block);
        }

        private ValueTask LoadBlock(StaticsBlock block)
        {
            try
            {
                int count = staticsFile.FillBlock(block.X, block.Y, block.Tiles);
                block.Initialize(staticsData, count);

                return staticsToSync.EnqueueAsync(block);
            }
            catch(Exception e)
            {
                throw;
            }
        }

        public void Dispose()
        {
            source.Cancel();

#pragma warning disable CA2012

            if (staticsToLoad.DisposeAsync() is { IsCompleted: false } task)
                task.GetAwaiter().GetResult();

#pragma warning restore CA2012
        }
    }
}
