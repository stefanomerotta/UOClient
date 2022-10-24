using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace UOClient.Effects
{
    public class DualTextureWithAlphaEffect
    {
        private readonly Effect effect;
        private readonly EffectParameter worldViewProjection;
        private readonly EffectParameter texture0;
        private readonly EffectParameter texture1;
        private readonly EffectParameter texture2;

        private Matrix world = Matrix.Identity;
        private Matrix view = Matrix.Identity;
        private Matrix projection = Matrix.Identity;

        /// <summary>
        /// Gets or sets the world matrix.
        /// </summary>
        public Matrix World { get => world; set { world = value; UpdateWorldViewProjection(); } }

        /// <summary>
        /// Gets or sets the view matrix.
        /// </summary>
        public Matrix View { get => view; set { view = value; UpdateWorldViewProjection(); } }

        /// <summary>
        /// Gets or sets the projection matrix.
        /// </summary>
        public Matrix Projection { get => projection; set { projection = value; UpdateWorldViewProjection(); } }

        /// <summary>
        /// Gets or sets the current base texture.
        /// </summary>
        public Texture2D Texture0 { get => texture0.GetValueTexture2D(); set => texture0.SetValue(value); }

        /// <summary>
        /// Gets or sets the current overlay texture.
        /// </summary>
        public Texture2D Texture1 { get => texture1.GetValueTexture2D(); set => texture1.SetValue(value); }

        /// <summary>
        /// Gets or sets the current alpha texture.
        /// </summary>
        public Texture2D Texture2 { get => texture2.GetValueTexture2D(); set => texture2.SetValue(value); }

        public EffectTechnique CurrentTechnique { get => effect.CurrentTechnique; set => effect.CurrentTechnique = value; }

        /// <summary>
        /// Creates a new DualTextureEffect with default parameter settings.
        /// </summary>
        public DualTextureWithAlphaEffect(ContentManager contentManager)
        {
            effect = contentManager.Load<Effect>("shaders/terrain");
            worldViewProjection = effect.Parameters["WorldViewProjection"];
            texture0 = effect.Parameters["Texture0"];
            texture1 = effect.Parameters["Texture1"];
            texture2 = effect.Parameters["Texture2"];
        }

        private void UpdateWorldViewProjection()
        {
            Matrix.Multiply(ref world, ref view, out Matrix worldView);
            Matrix.Multiply(ref worldView, ref projection, out Matrix worldViewProj);

            worldViewProjection.SetValue(worldViewProj);
        }
    }
}
