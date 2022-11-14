using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TexturePacker;

namespace UOClient.Utilities
{
    internal class TextureAtlas : Texture2D
    {
        private readonly Packer packer;

        public TextureAtlas(GraphicsDevice device, int width, int height)
            : base(device, width, height, false, SurfaceFormat.Dxt5)
        {
            packer = new(width, height);
        }

        public Rectangle Add(byte[] texture, int width, int height)
        {
            PackedRectangle bounds = packer.PackRect(width, height);

            Rectangle rect = new()
            {
                X = bounds.X,
                Y = bounds.Y,
                Width = bounds.Width,
                Height = bounds.Height
            };

            SetData(0, rect, texture, 0, texture.Length);

            return rect;
        }
    }
}
