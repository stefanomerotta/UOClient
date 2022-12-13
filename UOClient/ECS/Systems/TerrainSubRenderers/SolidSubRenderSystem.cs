using GameData.Structures.Contents.Terrains;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using UOClient.Data;
using UOClient.Effects;
using UOClient.Maps.Components;

namespace UOClient.ECS.Systems.Renderers
{
    internal sealed class SolidSubRenderSystem : IDisposable
    {
        private readonly GraphicsDevice device;
        private readonly SolidTerrainEffect solid;
        private readonly IsometricCamera camera;
        private readonly SolidTerrainData[] solidsData;
        private readonly TerrainTextureFile textureFile;
        private readonly Dictionary<int, Texture2D> textures;

        public bool IsEnabled { get; set; }

        public SolidSubRenderSystem(GraphicsDevice device, ContentManager contentManager, IsometricCamera camera,
            TerrainTextureFile textureFile, SolidTerrainData[] solidsData)
        {
            this.device = device;
            this.camera = camera;
            this.textureFile = textureFile;
            this.solidsData = solidsData;

            textures = new();

            ref readonly Matrix worldViewProjection = ref camera.WorldViewProjectionMatrix;

            solid = new(contentManager, in worldViewProjection);
        }

        public void Update()
        {
            solid.SetWorldViewProjection(in camera.WorldViewProjectionMatrix);
        }

        public void SetupDraw(in TerrainIndicesEntry entry)
        {
            ref SolidTerrainData data = ref solidsData[entry.TileId];
            if (data.Id == 0)
                return;

            solid.TextureIndex = entry.TileId;
            solid.Texture0 = GetTexture(data.Texture0.Id);
            solid.Texture1 = GetTexture(data.Texture1.Id);
            solid.AlphaMask = GetTexture(data.AlphaMask.Id);

            solid.Texture0Stretch = (int)data.Texture0.Stretch;
            solid.Texture1Stretch = (int)data.Texture1.Stretch;
            solid.AlphaMaskStretch = (int)data.AlphaMask.Stretch;

            EffectPass pass = solid.CurrentTechnique.Passes[0];
            pass.Apply();
        }

        private Texture2D GetTexture(int id)
        {
            if (textures.TryGetValue(id, out Texture2D? texture))
                return texture;

            byte[] data = textureFile.ReadTexture(id, out byte dxtFormat, out ushort width, out ushort height);

            SurfaceFormat format = dxtFormat switch
            {
                1 => SurfaceFormat.Dxt1,
                3 => SurfaceFormat.Dxt3,
                5 => SurfaceFormat.Dxt5,
                _ => throw new Exception("Unsupported Dxt compression")
            };

            Texture2D tex = new(device, width, height, false, format);
            tex.SetData(data);

            return textures[id] = tex;
        }

        public void Dispose()
        {
            solid.Dispose();

            foreach (Texture2D texture in textures.Values)
            {
                texture.Dispose();
            }
        }
    }
}
