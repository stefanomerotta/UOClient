using DefaultEcs;
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

            activeBlocks = world.GetEntities().AsMap<StaticsBlock>();

            ref readonly Matrix worldViewProjection = ref camera.WorldViewProjectionMatrix;

            statics = new(contentManager, in worldViewProjection)
            {
                Rotation = Matrix.CreateRotationY(MathHelper.ToRadians(45)),
            };
        }

        public void Update(GameTime state)
        {
            BlendState prevBlendState = device.BlendState;
            DepthStencilState prevDepthStencilState = device.DepthStencilState;

            statics.SetMatrices(in camera.WorldViewMatrix, in camera.WorldViewProjectionMatrix);

            EffectPass firstPass = statics.CurrentTechnique.Passes[0];
            EffectPass secondPass = statics.CurrentTechnique.Passes[1];

            foreach (StaticsBlock block in activeBlocks.Keys)
            {
                device.BlendState = BlendState.NonPremultiplied;
                device.DepthStencilState = DepthStencilState.Default;
                
                statics.Texture0 = block.Texture;
                statics.TextureSize = new(block.TextureWidth, block.TextureHeight);

                firstPass.Apply();
                block.Draw(device);

                //device.BlendState = BlendState.NonPremultiplied;
                //device.DepthStencilState = DepthStencilState.DepthRead;

                //secondPass.Apply();
                //block.Draw(device);
            }

            device.DepthStencilState = prevDepthStencilState;
            device.BlendState = prevBlendState;
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
