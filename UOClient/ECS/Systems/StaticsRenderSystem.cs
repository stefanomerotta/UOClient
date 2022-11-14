﻿using DefaultEcs;
using DefaultEcs.System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using UOClient.Effects;
using UOClient.Maps.Statics;

namespace UOClient.ECS.Systems
{
    internal sealed class StaticsRenderSystem : ISystem<GameTime>
    {
        private readonly GraphicsDevice device;
        private readonly EntityMap<StaticsBlock> activeBlocks;

        private readonly StaticsEffect statics;
        private readonly IsometricCamera camera;

        public bool IsEnabled { get; set; }

        public StaticsRenderSystem(World world, GraphicsDevice device, ContentManager contentManager, IsometricCamera camera)
        {
            this.camera = camera;
            this.device = device;

            activeBlocks = world.GetEntities()
                .With<StaticsBlock>()
                .AsMap<StaticsBlock>();

            ref readonly Matrix worldViewProjection = ref camera.WorldViewProjection;

            statics = new(contentManager, in worldViewProjection)
            {
                Rotation = Matrix.CreateRotationY(MathHelper.ToRadians(45))
            };
        }

        public void Update(GameTime state)
        {
            EffectPass pass = statics.CurrentTechnique.Passes[0];

            statics.SetWorldViewProjection(in camera.WorldViewProjection);
            
            pass.Apply();

            foreach (StaticsBlock block in activeBlocks.Keys)
            {
                statics.Texture0 = block.TextureArray;
                statics.TextureSize = new(TextureBuffer.MaxTextureWidth, TextureBuffer.MaxTextureHeight);

                block.Draw(device);
            }
        }

        public void Dispose()
        {
            foreach (StaticsBlock block in activeBlocks.Keys)
            {
                block.Dispose();
            }
        }
    }
}
