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
using UOClient.Maps.Statics;
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
        private readonly World world;
        private readonly EntityMap<Sector> terrains;

        private readonly AsyncCommandProcessor<TerrainBlock> blocksToLoad;
        private readonly CommandQueue<TerrainBlock> blocksToSync;
        private readonly CancellationTokenSource source;

        private readonly IDisposable sectorAddedSubscription;
        private readonly IDisposable sectorRemovedSubscription;

        private bool disposed;

        public bool IsEnabled { get; set; }

        public TerrainLoaderSystem(World world, GraphicsDevice device, TerrainFile terrainFile)
        {
            source = new();

            this.world = world;
            this.device = device;
            this.terrainFile = terrainFile;

            blocksToLoad = new(poolSize, LoadBlock, source.Token);
            blocksToSync = new(poolSize);

            terrains = world.GetEntities()
                .With<TerrainBlock>()
                .Without<StaticsBlock>()
                .AsMap<Sector>();

            sectorAddedSubscription = world.Subscribe<SectorAdded>(OnSectorAdded);
            sectorRemovedSubscription = world.Subscribe<SectorRemoved>(OnSectorRemoved);
        }

        public void Update(GameTime state)
        {
            while (blocksToSync.TryDequeue(out TerrainBlock? block))
            {
                block.SendToVRAM(device);

                Entity e = world.CreateEntity();

                e.Set(block);
                e.Set(new Sector(block.X, block.Y));
            }
        }

        private void OnSectorAdded(in SectorAdded sector)
        {
            if (!pool.TryGet(out TerrainBlock? block))
                block = new();

            block.X = sector.X;
            block.Y = sector.Y;

            blocksToLoad.TryEnqueue(block);
        }

        private void OnSectorRemoved(in SectorRemoved sector)
        {
            if (!terrains.TryGetEntity(UnsafeUtility.As<SectorRemoved, Sector>(in sector), out Entity e))
                return;

            TerrainBlock block = e.Get<TerrainBlock>();

            block.ClearVRAM();
            pool.Return(block);

            e.Dispose();
        }

        private ValueTask LoadBlock(TerrainBlock block)
        {
            terrainFile.FillBlock(block.X, block.Y, block.Tiles);
            block.Initialize();

            return blocksToSync.EnqueueAsync(block, source.Token);
        }

        public void Dispose()
        {
            if (disposed)
                return;

            disposed = true;

            source.Cancel();

            sectorAddedSubscription.Dispose();
            sectorRemovedSubscription.Dispose();

            blocksToSync.Dispose();

#pragma warning disable CA2012

            if (blocksToLoad.DisposeAsync() is { IsCompleted: false } task)
                task.GetAwaiter().GetResult();

#pragma warning restore CA2012
        }
    }
}
