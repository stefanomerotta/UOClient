using DefaultEcs;
using DefaultEcs.System;
using Microsoft.Xna.Framework;
using System;
using UOClient.Data;
using UOClient.ECS.Components;
using UOClient.ECS.Events;

namespace UOClient.ECS.Systems
{
    internal class SectorsSystem : ISystem<GameTime>
    {
        private readonly World world;
        private readonly EntityMultiMap<Sector> activeSectors;
        private readonly int blocksWidth;
        private readonly int blocksHeight;
        private readonly IDisposable sectorSubscription;

        public bool IsEnabled { get; set; }

        public SectorsSystem(World world, int mapWidth, int mapHeight)
        {
            this.world = world;

            blocksWidth = (int)Math.Ceiling(mapWidth / (double)TerrainFile.BlockSize);
            blocksHeight = (int)Math.Ceiling(mapHeight / (double)TerrainFile.BlockSize);

            activeSectors = world.GetEntities()
                .With<Sector>()
                .AsMultiMap<Sector>();

            sectorSubscription = world.Subscribe<CurrentSectorChanged>(OnCurrentSectorChanged);
        }

        public void Update(GameTime state)
        { }

        private void OnCurrentSectorChanged(in CurrentSectorChanged newSector)
        {
            const int activeRange = 1;
            const int disabledRange = activeRange + 1;
            const int size = activeRange * 2 + 1;

            Span<bool> area = stackalloc bool[size * size];

            foreach (Sector sector in activeSectors.Keys)
            {
                int deltaX = sector.X - newSector.X;
                int deltaY = sector.Y - newSector.Y;

                int absDeltaX = Math.Abs(deltaX);
                int absDeltaY = Math.Abs(deltaY);

                if (absDeltaX is > disabledRange || absDeltaY is > disabledRange)
                    world.Publish(new SectorRemoved(sector.X, sector.Y));

                else if (absDeltaX is <= activeRange && absDeltaY is <= activeRange)
                    area[deltaX + activeRange + (deltaY + activeRange) * size] = true;
            }

            for (int i = 0; i < area.Length; i++)
            {
                if (area[i])
                    continue;

                int x = newSector.X + (i % size) - 1;
                int y = newSector.Y + (i / size) - 1;

                if (x < 0 || x >= blocksWidth || y < 0 || y >= blocksHeight)
                    continue;

                world.Publish(new SectorAdded((ushort)x, (ushort)y));
            }
        }

        public void Dispose()
        {
            sectorSubscription.Dispose();
        }
    }
}
