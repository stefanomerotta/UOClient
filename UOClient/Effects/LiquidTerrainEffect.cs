using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace UOClient.Effects
{
    public class LiquidTerrainEffect
    {
        private readonly Effect effect;

        private readonly EffectParameter texture0;
        private readonly EffectParameter normal;
        private readonly EffectParameter texture0Stretch;
        private readonly EffectParameter normalStretch;
        private readonly EffectParameter waveHeight;
        private readonly EffectParameter windDirection;
        private readonly EffectParameter windForce;
        private readonly EffectParameter time;
        private readonly EffectParameter center;
        private readonly EffectParameter textureIndex;
        private readonly EffectParameter worldViewProjection;

        private bool followCenter;

        public Texture2D Texture0 { get => texture0.GetValueTexture2D(); set => texture0.SetValue(value); }
        public Texture2D Normal { get => normal.GetValueTexture2D(); set => normal.SetValue(value); }
        public int Texture0Stretch { get => texture0Stretch.GetValueInt32(); set => texture0Stretch.SetValue(value); }
        public int NormalStretch { get => normalStretch.GetValueInt32(); set => normalStretch.SetValue(value); }
        public int TextureIndex { get => textureIndex.GetValueInt32(); set => textureIndex.SetValue(value); }
        public float WaveHeight { get => waveHeight.GetValueSingle(); set => waveHeight.SetValue(value); }
        public Vector2 WindDirection { get => windDirection.GetValueVector2(); set => windDirection.SetValue(value); }
        public float WindForce { get => windForce.GetValueSingle(); set => windForce.SetValue(value); }
        public float Time { get => time.GetValueSingle(); set => time.SetValue(value); }
        public Vector2 Center { get => center.GetValueVector2(); set => center.SetValue(value); }
        public EffectTechnique CurrentTechnique => effect.CurrentTechnique;

        public bool FollowCenter
        {
            get => followCenter;
            set
            {
                followCenter = value;

                if (value)
                    effect.CurrentTechnique = effect.Techniques[(int)ShaderIndex.FollowCenter];
                else
                    effect.CurrentTechnique = effect.Techniques[(int)ShaderIndex.Main];
            }
        }

        public LiquidTerrainEffect(ContentManager contentManager, in Matrix worldViewProj)
        {
            effect = contentManager.Load<Effect>("shaders/liquid-terrain");

            texture0 = effect.Parameters["Texture0"];
            normal = effect.Parameters["Normal"];
            texture0Stretch = effect.Parameters["Texture0Stretch"];
            normalStretch = effect.Parameters["NormalStretch"];
            waveHeight = effect.Parameters["WaveHeight"];
            windDirection = effect.Parameters["WindDirection"];
            windForce = effect.Parameters["WindForce"];
            time = effect.Parameters["Time"];
            center = effect.Parameters["Center"];
            textureIndex = effect.Parameters["TextureIndex"];
            worldViewProjection = effect.Parameters["WorldViewProjection"];

            worldViewProjection.SetValue(worldViewProj);
        }

        public void SetWorldViewProjection(in Matrix matrix)
        {
            worldViewProjection.SetValue(matrix);
        }

        private enum ShaderIndex
        {
            Main = 0x0,
            FollowCenter = 0x1
        }
    }
}