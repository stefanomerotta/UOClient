using GameData.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Threading.Channels;
using UOClient.Data;
using UOClient.Effects;
using UOClient.Maps.Components;
using UOClient.Maps.Terrain;

namespace UOClient.Old
{
    internal class MapManager
    {
        private const int areaSize = 3;
        private const int halfAreaSize = areaSize / 2;

        private readonly Channel<MapBlock> readyToSyncBlocks;

        private readonly MapBlock?[,] blocks;
        private readonly TerrainFile mapFile;
        private readonly StaticsFile staticsFile;
        private readonly int blockMaxX;
        private readonly int blockMaxY;
        private readonly StaticData[] staticsData;
        private int currentBlockX;
        private int currentBlockY;
        private Bounds blockBounds;

        private IsometricCamera camera;
        private SolidTerrainEffect solid;
        private LiquidTerrainEffect liquid;
        private StaticsEffect statics;

        public MapManager(int width, int height)
        {
            readyToSyncBlocks = Channel.CreateBounded<MapBlock>
            (
                new BoundedChannelOptions(areaSize * areaSize)
                {
                    SingleReader = true,
                    SingleWriter = true,
                    FullMode = BoundedChannelFullMode.Wait,
                }
            );

            currentBlockX = -1;
            currentBlockY = -1;

            mapFile = new(width, height);
            staticsFile = new(width, height);

            blockMaxX = mapFile.BlocksWidth - 1;
            blockMaxY = mapFile.BlocksHeight - 1;

            blocks = new MapBlock[mapFile.BlocksWidth, mapFile.BlocksHeight];

            using StaticsDataFile staticsDataFile = new();
            staticsData = staticsDataFile.Load(false);
        }

        public void Initialize(ContentManager contentManager, IsometricCamera camera)
        {
            this.camera = camera;

            SolidTerrainInfo.Load(contentManager);
            LiquidTerrainInfo.Load(contentManager);

            ref readonly Matrix worldViewProjection = ref camera.WorldViewProjection;

            solid = new(contentManager, in worldViewProjection);
            liquid = new(contentManager, in worldViewProjection);
            statics = new(contentManager, in worldViewProjection);
        }

        public void OnLocationChanged()
        {
            ref readonly Matrix worldViewProjection = ref camera.WorldViewProjection;

            solid.SetWorldViewProjection(in worldViewProjection);
            liquid.SetWorldViewProjection(in worldViewProjection);
            statics.SetWorldViewProjection(in worldViewProjection);
        }

        public void OnSectorChanged(int newBlockX, int newBlockY)
        {
            //Vector3 target = camera.Target;

            //int newBlockX = (int)target.X >> MapFile.BlockSizeShift;
            //int newBlockY = (int)target.Z >> MapFile.BlockSizeShift;

            //if (blockX == newBlockX && blockY == newBlockY)
            //    return;

            currentBlockX = newBlockX;
            currentBlockY = newBlockY;

            blockBounds = new
            (
                Math.Clamp(currentBlockX - halfAreaSize, 0, blockMaxX),
                Math.Clamp(currentBlockY - halfAreaSize, 0, blockMaxY),
                Math.Clamp(currentBlockX + halfAreaSize + 1, 0, blockMaxX),
                Math.Clamp(currentBlockY + halfAreaSize + 1, 0, blockMaxY)
            );

            UnloadInactiveBlocks();
            LoadActiveBlocks();
        }

        public void TEMP(GraphicsDevice device)
        {
            for (int y = blockBounds.StartY; y < blockBounds.EndY; y++)
            {
                for (int x = blockBounds.StartX; x < blockBounds.EndX; x++)
                {
                    ref MapBlock? block = ref blocks[x, y];
                    block.SendToVRAM(device);
                }
            }
        }

        public void Draw(GraphicsDevice device, GameTime gameTime)
        {
            DrawSolid(device);
            DrawLiquid(device, gameTime);
            DrawStatics(device);
        }

        private void DrawSolid(GraphicsDevice device)
        {
            EffectPass pass = solid.CurrentTechnique.Passes[0];

            for (int k = 1; k < (int)LandTileId.Water; k++)
            {
                ref SolidTerrainInfo info = ref SolidTerrainInfo.Values[k];

                solid.TextureIndex = k;
                solid.Texture0 = info.Texture0;
                solid.Texture1 = info.Texture1;
                solid.AlphaMask = info.AlphaMask;

                solid.Texture0Stretch = info.Texture0Stretch;
                solid.Texture1Stretch = info.Texture1Stretch;
                solid.AlphaMaskStretch = info.AlphaMaskStretch;

                pass.Apply();

                for (int x = blockBounds.StartX; x < blockBounds.EndX; x++)
                {
                    for (int y = blockBounds.StartY; y < blockBounds.EndY; y++)
                    {
                        blocks[x, y]!.Terrain.Draw(device, k);
                    }
                }
            }
        }

        private void DrawLiquid(GraphicsDevice device, GameTime gameTime)
        {
            EffectPass waterPass;
            Vector3 target = camera.Target;

            liquid.Time = (float)gameTime.TotalGameTime.TotalMilliseconds / 100.0f;
            liquid.Center = new Vector2(target.X, target.Z);

            for (int k = (int)LandTileId.Water; k < (int)LandTileId.Length; k++)
            {
                ref LiquidTerrainInfo info = ref LiquidTerrainInfo.Values[k];

                liquid.TextureIndex = k;
                liquid.Texture0 = info.Texture0;
                liquid.Texture0Stretch = info.Texture0Stretch;

                liquid.Normal = info.Normal;
                liquid.NormalStretch = info.NormalStretch;

                liquid.WaveHeight = info.WaveHeight;

                if (info.WindSpeed == 0)
                {
                    liquid.WindForce = 0.05f;
                    liquid.WindDirection = new(0, 1);
                }
                else
                {
                    liquid.WindForce = info.WindSpeed;
                    liquid.WindDirection = new(1, 0);
                }

                liquid.FollowCenter = info.FollowCenter;

                waterPass = liquid.CurrentTechnique.Passes[0];
                waterPass.Apply();

                for (int i = blockBounds.StartX; i < blockBounds.EndX; i++)
                {
                    for (int j = blockBounds.StartY; j < blockBounds.EndY; j++)
                    {
                        blocks[i, j]!.Terrain.Draw(device, k);
                    }
                }
            }
        }

        private void DrawStatics(GraphicsDevice device)
        {
            EffectPass pass = statics.CurrentTechnique.Passes[0];

            pass.Apply();

            for (int x = blockBounds.StartX; x < blockBounds.EndX; x++)
            {
                for (int y = blockBounds.StartY; y < blockBounds.EndY; y++)
                {
                    blocks[x, y]!.Statics.Draw(device);
                }
            }
        }

        private void LoadActiveBlocks()
        {
            for (int y = blockBounds.StartY; y < blockBounds.EndY; y++)
            {
                for (int x = blockBounds.StartX; x < blockBounds.EndX; x++)
                {
                    ref MapBlock? block = ref blocks[x, y];

                    if (block is null)
                    {
                        if (!MapBlock.Pool.TryGet(out block))
                            block = new();

                        block.Initialize(x, y, mapFile, staticsFile, staticsData);
                        readyToSyncBlocks.Writer.TryWrite(block);
                    }
                }
            }
        }

        private void UnloadInactiveBlocks()
        {
            int startX = blockBounds.StartX - 1;
            int startY = blockBounds.StartY - 1;
            int endX = blockBounds.EndX + 1;
            int endY = blockBounds.EndY + 1;

            for (int k = 0; k < areaSize + 2; k++)
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

                block.CleanUp();
                readyToSyncBlocks.Writer.TryWrite(block);

                //MapBlock.Pool.Return(block);
                //block = null;
            }
        }
    }
}
