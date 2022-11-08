using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using UOClient.Data;
using UOClient.Maps.Components;

namespace UOClient.Maps
{
    internal class MapManager
    {
        private const int size = 3;
        private const int halfSize = size / 2;

        private readonly MapBlock?[,] blocks;
        private readonly MapFile map;
        private int blockX;
        private int blockY;
        private Bounds blockBounds;

        private GraphicsDevice device;
        private IsometricCamera camera;

        private readonly int blockMaxX;
        private readonly int blockMaxY;

        public MapManager(int width, int height)
        {
            blockX = -1;
            blockY = -1;

            map = new MapFile(width, height);

            blockMaxX = map.BlocksWidth - 1;
            blockMaxY = map.BlocksHeight - 1;

            blocks = new MapBlock[map.BlocksWidth, map.BlocksHeight];
        }

        public void Initialize(GraphicsDevice device, ContentManager contentManager, IsometricCamera camera)
        {
            this.device = device;
            this.camera = camera;
        }

        public void OnLocationChanged()
        {
            Vector3 target = camera.Target;

            int newBlockX = (int)target.X >> MapFile.BlockSizeShift;
            int newBlockY = (int)target.Z >> MapFile.BlockSizeShift;

            if (blockX == newBlockX && blockY == newBlockY)
                return;

            blockX = newBlockX;
            blockY = newBlockY;

            blockBounds = new
            (
                Math.Clamp(blockX - halfSize, 0, blockMaxX),
                Math.Clamp(blockY - halfSize, 0, blockMaxY),
                Math.Clamp(blockX + halfSize + 1, 0, blockMaxX),
                Math.Clamp(blockY + halfSize + 1, 0, blockMaxY)
            );

            LoadBlocks();
            UnloadUnusedBlocks();
        }

        private void LoadBlocks()
        {
            for (int y = blockBounds.StartY; y < blockBounds.EndY; y++)
            {
                for (int x = blockBounds.StartX; x < blockBounds.EndX; x++)
                {
                    blocks[x, y] ??= new(x, y, map);
                }
            }
        }

        private void UnloadUnusedBlocks()
        {
            int startX = blockBounds.StartX - 1;
            int startY = blockBounds.StartY - 1;
            int endX = blockBounds.EndX + 1;
            int endY = blockBounds.EndY + 1;

            for (int k = 0; k < size + 2; k++)
            {
                Unload(startX + k, startY);
                Unload(startX + k, endY);
                Unload(startX, startY + k);
                Unload(endX, startY + k);
            }

            void Unload(int x, int y)
            {
                if (x < 0 || x > blockMaxX)
                    return;

                if (y < 0 || y > blockMaxY)
                    return;

                ref MapBlock? block = ref blocks[x, y];
                if (block is null)
                    return;

                block.Dispose();
                block = null;
            }
        }
    }
}
