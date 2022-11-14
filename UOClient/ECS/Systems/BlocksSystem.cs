using DefaultEcs;
using DefaultEcs.System;
using Microsoft.Xna.Framework;
using System;
using UOClient.Data;
using UOClient.ECS.Components;
using UOClient.ECS.Events;

namespace UOClient.ECS.Systems
{
    internal class BlocksSystem : ISystem<GameTime>
    {
        private readonly World world;
        private readonly EntityMap<Block> activeBlocks;
        private readonly int blocksWidth;
        private readonly int blocksHeight;
        private readonly IDisposable blockSubscription;

        public bool IsEnabled { get; set; }

        public BlocksSystem(World world, int mapWidth, int mapHeight)
        {
            this.world = world;

            blocksWidth = (int)Math.Ceiling(mapWidth / (double)TerrainFile.BlockSize);
            blocksHeight = (int)Math.Ceiling(mapHeight / (double)TerrainFile.BlockSize);

            activeBlocks = world.GetEntities()
                .AsMap<Block>();

            blockSubscription = world.Subscribe<CurrentSectorChanged>(OnCurrentSectorChanged);
        }

        public void Update(GameTime state)
        { }

        private void OnCurrentSectorChanged(in CurrentSectorChanged newSector)
        {
            const int activeRange = 1;
            const int disabledRange = activeRange + 1;
            const int size = activeRange * 2 + 1;

            Span<bool> area = stackalloc bool[size * size];

            foreach (Block block in activeBlocks.Keys)
            {
                int deltaX = block.X - newSector.X;
                int deltaY = block.Y - newSector.Y;

                int absDeltaX = Math.Abs(deltaX);
                int absDeltaY = Math.Abs(deltaY);

                if (absDeltaX is > disabledRange || absDeltaY is > disabledRange)
                    activeBlocks[block].Dispose();

                else if (absDeltaX is <= activeRange && absDeltaY is <= activeRange)
                    area[deltaX + activeRange + (deltaY + activeRange) * size] = true;
            }

            for (int i = 0; i < area.Length; i++)
            {
                if (area[i])
                    continue;

                ushort x = (ushort)(newSector.X + (i % size) - 1);
                ushort y = (ushort)(newSector.Y + (i / size) - 1);

                if (x < 0 || x >= blocksWidth || y < 0 || y >= blocksHeight)
                    continue;

                Entity e = world.CreateEntity();
                e.Set(new Block(x, y));
            }
        }

        public void Dispose()
        {
            blockSubscription.Dispose();
        }
    }
}
