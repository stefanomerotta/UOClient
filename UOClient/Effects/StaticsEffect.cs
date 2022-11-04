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
        private readonly EffectParameter worldViewProjection;

        private Matrix world = Matrix.Identity;
        private Matrix view = Matrix.Identity;
        private Matrix projection = Matrix.Identity;

        public Matrix World { get => world; init => world = value; }
        public Matrix View { get => view; set => SetViewMatrix(value); }
        public Matrix Projection { get => projection; init => projection = value; }

        public Texture2D Texture0
        {
            get => texture0.GetValueTexture2D();
            set => texture0.SetValue(value);
        }

        public Vector2 TextureSize
        {
            get => textureSize.GetValueVector2();
            set => textureSize.SetValue(value);
        }

        public EffectTechnique CurrentTechnique => effect.CurrentTechnique;

        public StaticsEffect(ContentManager contentManager)
        {
            effect = contentManager.Load<Effect>("shaders/statics");

            texture0 = effect.Parameters["Texture0"];
            textureSize = effect.Parameters["TextureSize"];
            worldViewProjection = effect.Parameters["WorldViewProjection"];
        }

        private void SetViewMatrix(Matrix view)
        {
            this.view = view;

            Matrix.Multiply(ref world, ref view, out Matrix worldView);
            Matrix.Multiply(ref worldView, ref projection, out Matrix worldViewProj);

            worldViewProjection.SetValue(worldViewProj);
        }
    }
}
