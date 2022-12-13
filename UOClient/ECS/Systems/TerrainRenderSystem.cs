using DefaultEcs;
using DefaultEcs.System;
using GameData.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using UOClient.Data;
using UOClient.Maps.Components;
using UOClient.Maps.Terrain;

namespace UOClient.ECS.Systems.Renderers
{
    internal sealed class TerrainRenderSystem : ISystem<GameTime>
    {
        private readonly GraphicsDevice device;
        private readonly EntityMap<TerrainBlock> activeBlocks;

        private readonly TerrainData terrainData;
        private readonly LiquidSubRenderSystem liquidRenderer;
        private readonly SolidSubRenderSystem solidRenderer;
        private readonly SingleSubRenderSystem singleRenderer;

        public bool IsEnabled { get; set; }

        public TerrainRenderSystem(World world, GraphicsDevice device, ContentManager contentManager, IsometricCamera camera,
            TerrainTextureFile textureFile, in TerrainData terrainData)
        {
            this.device = device;
            this.terrainData = terrainData;

            activeBlocks = world.GetEntities().AsMap<TerrainBlock>();

            liquidRenderer = new(device, contentManager, camera, textureFile, terrainData.Liquid);
            solidRenderer = new(device, contentManager, camera, textureFile, terrainData.Solid);
            singleRenderer = new(device, contentManager, camera, textureFile, terrainData.Single);
        }

        public void Update(GameTime state)
        {
            liquidRenderer.Update(state);
            solidRenderer.Update();
            singleRenderer.Update();

            foreach (TerrainBlock block in activeBlocks.Keys)
            {
                for (int k = 0; k < block.IndicesCount; k++)
                {
                    TerrainIndicesEntry entry = block.Indices[k];

                    switch(terrainData.Types[entry.TileId])
                    {
                        case TerrainTileType.Liquid: liquidRenderer.SetupDraw(in entry); break;
                        case TerrainTileType.Solid: solidRenderer.SetupDraw(in entry); break;
                        case TerrainTileType.Single: singleRenderer.SetupDraw(in entry); break;
                        default: continue;
                    }

                    block.Draw(device, k);
                }
            }
        }

        public void Dispose()
        {
            activeBlocks.Dispose();
            liquidRenderer.Dispose();
            solidRenderer.Dispose();
            singleRenderer.Dispose();

            foreach (TerrainBlock block in activeBlocks.Keys)
            {
                block.Dispose();
            }
        }
    }
}
