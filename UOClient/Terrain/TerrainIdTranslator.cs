using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace UOClient.Terrain
{
    public class TerrainIdTranslator
    {
        private readonly Texture2D[] textures = new Texture2D[0x4000];
        private readonly ContentManager contentManager;

        public TerrainIdTranslator(ContentManager contentManager)
        {
            this.contentManager = contentManager;

            textures[0] = contentManager.Load<Texture2D>("/land/water.dds");
        }

        public Texture2D GetTexture(ushort id)
        {
            if (textures[id] is null)
                LoadTexture(id);

            return textures[id];
        }

        private void LoadTexture(ushort id)
        {
            textures[id] = id switch
            {
                3 or 4 or 5 or 6 => contentManager.Load<Texture2D>("/land/grass-light.dds"),
                _ => textures[0],
            };
        }
    }
}
