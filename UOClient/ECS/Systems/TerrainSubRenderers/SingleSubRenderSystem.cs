using GameData.Structures.Contents.Terrains;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using UOClient.Data;
using UOClient.Effects;
using UOClient.Maps.Components;
using UOClient.Utilities;

namespace UOClient.ECS.Systems.Renderers
{
    internal sealed class SingleSubRenderSystem : IDisposable
    {
        private readonly SingleTerrainData[] terrainData;
        private readonly SingleTerrainEffect single;
        private readonly IsometricCamera camera;
        private readonly Dictionary<int, int> loadedTextures;
        private readonly TextureArray textureArray;

        public bool IsEnabled { get; set; }

        public SingleSubRenderSystem(GraphicsDevice device, ContentManager contentManager, IsometricCamera camera,
            TerrainTextureFile textureFile, SingleTerrainData[] terrainData)
        {
            this.camera = camera;
            this.terrainData = terrainData;

            loadedTextures = new();

            textureArray = new TextureArray(device, 64, 64, terrainData.Length);
            byte[] buffer = new byte[64 * 64];

            for (int i = 0; i < terrainData.Length; i++)
            {
                int textureId = terrainData[i].TextureId;
                if (textureId == 0)
                    continue;

                textureFile.FillTexture(textureId, buffer, out byte dxtFormat);

                if (dxtFormat != 5)
                    throw new Exception("Unsupported DXT compression");

                textureArray.Add(i, buffer);
                loadedTextures.Add(textureId, i);
            }

            ref readonly Matrix worldViewProjection = ref camera.WorldViewProjectionMatrix;

            single = new(contentManager, in worldViewProjection)
            {
                Texture0 = textureArray
            };
        }

        public void Update()
        {
            single.SetWorldViewProjection(in camera.WorldViewProjectionMatrix);
        }

        public void SetupDraw(in TerrainIndicesEntry entry)
        {
            if (entry.TileId > terrainData.Length)
                return;

            ref SingleTerrainData data = ref terrainData[entry.TileId];
            if (data.Id == 0)
                return;

            EffectPass pass = single.CurrentTechnique.Passes[0];
            pass.Apply();
        }

        public void Dispose()
        {
            single.Dispose();
            textureArray.Dispose();
        }
    }
}
