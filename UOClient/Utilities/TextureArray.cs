using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace UOClient.Utilities
{
    public class TextureArray : Texture2D
    {
        public TextureArray(GraphicsDevice graphicsDevice, int width, int height, int arraySize)
            : base(graphicsDevice, width, height, true, SurfaceFormat.Dxt5, SurfaceType.Texture, false, arraySize)
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

        //public static TextureArray LoadFromContentFolder(GraphicsDevice graphicsDevice, int widthPerTex, int heightPerTex, string path)
        //{
        //    var paths = Directory.GetFiles(Environment.CurrentDirectory + @"\Content\" + path);

        //    TextureArray pTexArray = new TextureArray(graphicsDevice, widthPerTex, heightPerTex, paths.Length);

        //    int index = 0;

        //    foreach (var file in paths)
        //        pTexArray.Add(index++, Content.Load<Texture2D>(path + @"\" + Path.GetFileNameWithoutExtension(file)));

        //    return pTexArray;
        //}
    }
}
