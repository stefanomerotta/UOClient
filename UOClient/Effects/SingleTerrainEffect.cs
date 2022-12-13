using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace UOClient.Effects
{
    public sealed class SingleTerrainEffect : IDisposable
    {
        private readonly Effect effect;

        private readonly EffectParameter texture0;
        private readonly EffectParameter worldViewProjection;

        public Texture2D Texture0 { get => texture0.GetValueTexture2D(); set => texture0.SetValue(value); }
        public EffectTechnique CurrentTechnique { get => effect.CurrentTechnique; set => effect.CurrentTechnique = value; }

        public SingleTerrainEffect(ContentManager contentManager, in Matrix worldviewProj)
        {
            effect = contentManager.Load<Effect>("shaders/single-terrain");

            texture0 = effect.Parameters["Texture0"];
            worldViewProjection = effect.Parameters["WorldViewProjection"];

            worldViewProjection.SetValue(worldviewProj);
        }

        public void SetWorldViewProjection(in Matrix matrix)
        {
            worldViewProjection.SetValue(matrix);
        }

        public void Dispose()
        {
            effect.Dispose();
        }
    }
}