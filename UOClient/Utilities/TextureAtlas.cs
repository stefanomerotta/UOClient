using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TexturePacker;

namespace UOClient.Utilities
{
    internal sealed class TextureAtlas : Texture2D
    {
        private readonly Packer packer;

        public TextureAtlas(GraphicsDevice device, int width, int height, SurfaceFormat format = SurfaceFormat.Dxt5)
            : base(device, width, height, false, format)
        {
            packer = new(width, height);
        }

        public Rectangle Add(byte[] texture, int width, int height)
        {
            Rectangle rect = UnsafeUtility.As<PackedRectangle, Rectangle>(packer.Pack(width, height));
            SetData(0, rect, texture, 0, texture.Length);

            return rect;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (!disposing)
                return;

            packer.Dispose();
        }
    }
}
