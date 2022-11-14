using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace UOClient.Utilities
{
    public class TextureArray : Texture2D
    {
        public TextureArray(GraphicsDevice graphicsDevice, int width, int height, int arraySize)
            : base(graphicsDevice, width, height, false, SurfaceFormat.Dxt5, arraySize)
        { }

        public void Add(int index, Texture2D texture)
        {
            for (int i = 0; i < texture.LevelCount; i++)
            {
                float divisor = 1.0f / (1 << i);
                byte[] pixelData = new byte[(int)(texture.Width * texture.Height * divisor * divisor)];
                Rectangle rect = new(0, 0, (int)(texture.Width * divisor), (int)(texture.Height * divisor));

                texture.GetData(i, 0, rect, pixelData, 0, pixelData.Length);
                SetData(i, index, rect, pixelData, 0, pixelData.Length);
            }
        }

        public void Add(int index, byte[] texture, int width, int height)
        {
            Rectangle rect = new(0, 0, width, height);
            SetData(0, index, rect, texture, 0, texture.Length);
        }
    }
}
