﻿using DefaultEcs;
using DefaultEcs.System;
using GameData.Structures.Contents.Terrains;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UOClient.Data;
using UOClient.ECS.Components;
using UOClient.Maps.Components;
using UOClient.Maps.Terrain;
using UOClient.Utilities;
using UOClient.Utilities.SingleThreaded;

namespace UOClient.ECS.Systems
{
    internal sealed class TerrainLoaderSystem : ISystem<GameTime>
    {
        private const int poolSize = 25;

        private static readonly ObjectPool<TerrainBlock> pool = new(poolSize);

        private readonly GraphicsDevice device;
        private readonly TerrainFile terrainFile;
        private readonly EntityMap<Block> blocks;
        private readonly int terrainTypeCount;

        private readonly AsyncCommandProcessor<TerrainBlock> blocksToLoad;
        private readonly CommandQueue<TerrainBlock> blocksToSync;
        private readonly CancellationTokenSource source;

        public bool IsEnabled { get; set; }

        public TerrainLoaderSystem(World world, GraphicsDevice device, TerrainFile terrainFile, in TerrainData terrainData)
        {
            source = new();

            this.device = device;
            this.terrainFile = terrainFile;
            terrainTypeCount = terrainData.MaxLength;

            blocksToLoad = new(poolSize, LoadBlock, source);
            blocksToSync = new(poolSize);

            blocks = world.GetEntities().AsMap<Block>();

            blocks.EntityAdded += OnBlockAdded;
            blocks.EntityRemoved += OnBlockRemoved;
        }

        public void Update(GameTime state)
        {
            while (blocksToSync.TryDequeue(out TerrainBlock? block))
            {
                if (!blocks.TryGetEntity(new(block.X, block.Y), out Entity e))
                {
                    pool.Return(block);
                    continue;
                }

                e.Set(block);
                block.SendToVRAM(device);
            }
        }

        private void OnBlockAdded(in Entity e)
        {
            if (!pool.TryGet(out TerrainBlock? block))
                block = new(terrainTypeCount);

            Block blockComponent = e.Get<Block>();

            block.X = blockComponent.X;
            block.Y = blockComponent.Y;

            blocksToLoad.TryEnqueue(block);
        }

        private void OnBlockRemoved(in Entity e)
        {
            if (!e.Has<TerrainBlock>())
                return;

            TerrainBlock block = e.Get<TerrainBlock>();

            block.ClearVRAM();
            pool.Return(block);
        }

        private ValueTask LoadBlock(TerrainBlock block)
        {
            terrainFile.FillBlock(block.X, block.Y, block.Tiles);
            block.Initialize();

            return blocksToSync.EnqueueAsync(block, source.Token);
        }

        public void Dispose()
        {
            source.Cancel();

            blocks.Dispose();
            blocksToSync.Dispose();
            blocksToLoad.Dispose();
        }
    }
}
