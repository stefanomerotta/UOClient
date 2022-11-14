using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace UOClient.Effects
{
    public class SolidTerrainEffect
    {
        private readonly Effect effect;

        private readonly EffectParameter texture0;
        private readonly EffectParameter texture1;
        private readonly EffectParameter alphaMask;

        private readonly EffectParameter texture0Stretch;
        private readonly EffectParameter texture1Stretch;
        private readonly EffectParameter alphaMaskStretch;

        private readonly EffectParameter textureIndex;
        private readonly EffectParameter worldViewProjection;

        public Texture2D Texture0 { get => texture0.GetValueTexture2D(); set => texture0.SetValue(value); }
        public Texture2D Texture1 { get => texture1.GetValueTexture2D(); set => texture1.SetValue(value); }
        public Texture2D AlphaMask { get => alphaMask.GetValueTexture2D(); set => alphaMask.SetValue(value); }
        public int Texture0Stretch { get => texture0Stretch.GetValueInt32(); set => texture0Stretch.SetValue(value); }
        public int Texture1Stretch { get => texture1Stretch.GetValueInt32(); set => texture1Stretch.SetValue(value); }
        public int AlphaMaskStretch { get => alphaMaskStretch.GetValueInt32(); set => alphaMaskStretch.SetValue(value); }
        public int TextureIndex { get => textureIndex.GetValueInt32(); set => textureIndex.SetValue(value); }
        public EffectTechnique CurrentTechnique { get => effect.CurrentTechnique; set => effect.CurrentTechnique = value; }

        public SolidTerrainEffect(ContentManager contentManager, in Matrix worldviewProj)
        {
            effect = contentManager.Load<Effect>("shaders/solid-terrain");

            texture0 = effect.Parameters["Texture0"];
            texture1 = effect.Parameters["Texture1"];
            alphaMask = effect.Parameters["AlphaMask"];
            texture0Stretch = effect.Parameters["Texture0Stretch"];
            texture1Stretch = effect.Parameters["Texture1Stretch"];
            alphaMaskStretch = effect.Parameters["AlphaMaskStretch"];
            textureIndex = effect.Parameters["TextureIndex"];
            worldViewProjection = effect.Parameters["WorldViewProjection"];

            worldViewProjection.SetValue(worldviewProj);
        }

        public void SetWorldViewProjection(in Matrix matrix)
        {
            worldViewProjection.SetValue(matrix);
        }
    }
}