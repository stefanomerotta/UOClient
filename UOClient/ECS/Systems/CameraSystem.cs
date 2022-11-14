using DefaultEcs;
using DefaultEcs.System;
using Microsoft.Xna.Framework;
using UOClient.Data;
using UOClient.ECS.Events;

namespace UOClient.ECS.Systems
{
    internal sealed class CameraSystem : ISystem<GameTime>
    {
        private const int sectorSize = TerrainFile.BlockSize;

        private readonly IsometricCamera camera;
        private readonly World world;
        
        private int sectorX;
        private int sectorY;

        public bool IsEnabled { get; set; }

        public CameraSystem(World world, IsometricCamera camera)
        {
            this.world = world;
            this.camera = camera;

            sectorX = (int)camera.Target.X / sectorSize;
            sectorY = (int)camera.Target.Y / sectorSize;
        }

        public void Update(GameTime state)
        {
            bool modified = camera.HandleKeyboardInput();
            if (!modified)
                return;

            Vector3 target = camera.Target;

            int newSectorX = (int)target.X / sectorSize;
            int newSectorY = (int)target.Z / sectorSize;

            if (newSectorX == sectorX && newSectorY == sectorY)
                return;

            sectorX = newSectorX;
            sectorY = newSectorY;

            world.Publish(new CurrentSectorChanged((ushort)newSectorX, (ushort)newSectorY));
        }

        public void Dispose()
        { }
    }
}
