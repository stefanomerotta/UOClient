using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using UOClient.Data;
using UOClient.Structures;
using UOClient.Terrain;

namespace UOClient.Maps
{
    internal class MapManager
    {
        private const int size = 3;
        private const int halfSize = size / 2;

        private readonly MapBlock?[,] blocks;
        private readonly Map map;
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

            blockMaxX = width / TerrainBlock.Size - 1;
            blockMaxY = height / TerrainBlock.Size - 1;
            map = new Map(width, height);

            blocks = new MapBlock[blockMaxX + 1, blockMaxY + 1];
        }

        public void Initialize(GraphicsDevice device, ContentManager contentManager, IsometricCamera camera)
        {
            this.device = device;
            this.camera = camera;
        }

        public void OnLocationChanged()
        {
            Vector3 target = camera.Target;

            int newBlockX = (int)target.X >> TerrainBlock.SizeOffset;
            int newBlockY = (int)target.Z >> TerrainBlock.SizeOffset;

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

            LoadBlocks(device);
            UnloadUnusedBlocks();
        }

        private void LoadBlocks(GraphicsDevice device)
        {
            for (int y = blockBounds.StartY; y < blockBounds.EndY; y++)
            {
                for (int x = blockBounds.StartX; x < blockBounds.EndX; x++)
                {
                    //blocks[x, y] ??= new(device, x, y, GetTiles(x, y));
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
