using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace UOClient.Effects
{
    internal class StaticsEffect
    {
        private readonly Effect effect;

        private readonly EffectParameter texture0;
        private readonly EffectParameter textureSize;
        private readonly EffectParameter planes;
        private readonly EffectParameter worldViewProjection;
        private readonly EffectParameter rotation;

        public Matrix Rotation { get => rotation.GetValueMatrix(); set => rotation.SetValue(value); }
        public Texture2D Texture0 { get => texture0.GetValueTexture2D(); set => texture0.SetValue(value); }
        public Vector2 TextureSize { get => textureSize.GetValueVector2(); set => textureSize.SetValue(value); }
        public Vector2 Planes { get => planes.GetValueVector2(); set => planes.SetValue(value); }
        public EffectTechnique CurrentTechnique => effect.CurrentTechnique;

        public StaticsEffect(ContentManager contentManager, in Matrix worldViewProj)
        {
            effect = contentManager.Load<Effect>("shaders/statics");

            texture0 = effect.Parameters["Texture0"];
            textureSize = effect.Parameters["TextureSize"];
            planes = effect.Parameters["Planes"];
            worldViewProjection = effect.Parameters["WorldViewProjection"];
            rotation = effect.Parameters["Rotation"];

            worldViewProjection.SetValue(worldViewProj);
        }

        public void SetWorldViewProjection(in Matrix worldViewProjection)
        {
            this.worldViewProjection.SetValue(worldViewProjection);
        }
    }
}
