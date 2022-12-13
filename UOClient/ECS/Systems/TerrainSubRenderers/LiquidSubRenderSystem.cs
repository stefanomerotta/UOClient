using GameData.Enums;
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
    internal sealed class LiquidSubRenderSystem : IDisposable
    {
        private readonly GraphicsDevice device;
        private readonly LiquidTerrainEffect liquid;
        private readonly IsometricCamera camera;
        private readonly LiquidTerrainData[] liquidsData;
        private readonly TerrainTextureFile textureFile;
        private readonly Dictionary<int, Texture2D> textures;

        public bool IsEnabled { get; set; }

        public LiquidSubRenderSystem(GraphicsDevice device, ContentManager contentManager, IsometricCamera camera,
            TerrainTextureFile textureFile, LiquidTerrainData[] liquidsData)
        {
            this.device = device;
            this.camera = camera;
            this.textureFile = textureFile;
            this.liquidsData = liquidsData;

            textures = new();

            ref readonly Matrix worldViewProjection = ref camera.WorldViewProjectionMatrix;

            liquid = new(contentManager, in worldViewProjection);
        }

        public void Update(GameTime gameTime)
        {
            liquid.SetWorldViewProjection(in camera.WorldViewProjectionMatrix);

            Vector3 target = camera.Target;

            liquid.Time = (float)gameTime.TotalGameTime.TotalMilliseconds / 100.0f;
            liquid.Center = new Vector2(target.X, target.Z);
            liquid.WindDirection = new(0, 1);
        }

        public void SetupDraw(in TerrainIndicesEntry entry)
        {
            ref LiquidTerrainData data = ref liquidsData[entry.TileId];
            if (data.Id == 0)
                return;

            liquid.TextureIndex = entry.TileId;
            liquid.WaveHeight = data.WaveHeight;
            liquid.WindForce = data.Speed != 0 ? data.Speed : 0.1f;
            liquid.FollowCenter = data.Type.Has(TerrainTileType.FollowCenter);

            liquid.Normal = GetTexture(data.Normal.Id);
            liquid.Texture0 = GetTexture(data.Texture0.Id);

            liquid.Texture0Stretch = (int)data.Texture0.Stretch;
            liquid.NormalStretch = (int)data.Normal.Stretch;

            EffectPass waterPass = liquid.CurrentTechnique.Passes[0];
            waterPass.Apply();
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
            liquid.Dispose();

            foreach (Texture2D texture in textures.Values)
            {
                texture.Dispose();
            }
        }
    }
}
