using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace UOClient.Utilities
{
    public class TextureArray : Texture2D
    {
        private readonly Rectangle bound;

        public TextureArray(GraphicsDevice graphicsDevice, int width, int height, int arraySize, SurfaceFormat format = SurfaceFormat.Dxt5)
            : base(graphicsDevice, width, height, false, format, arraySize)
        {
            bound = new(0, 0, width, height);
        }

        public void Add(int index, byte[] texture)
        {
            SetData(0, index, bound, texture, 0, texture.Length);
        }
    }
}
