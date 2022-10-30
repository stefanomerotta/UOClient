using GameData.Enums;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.InteropServices;

namespace UOClient.Terrain
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal readonly struct TerrainInfo
    {
        public static readonly TerrainInfo[] Values = new TerrainInfo[(int)LandTileId.Length];
        
        public readonly Texture2D Texture0;
        public readonly Texture2D Texture1;
        public readonly Texture2D AlphaMask;
               
        public readonly byte Texture0Stretch;
        public readonly byte Texture1Stretch;
        public readonly byte AlphaMaskStretch;

        private TerrainInfo(Texture2D texture0, Texture2D texture1, Texture2D alphaMask,
            byte textureStretch0, byte textureStretch1, byte alphaStretch)
        {
            Texture0 = texture0;
            Texture1 = texture1;
            AlphaMask = alphaMask;

            Texture0Stretch = textureStretch0;
            Texture1Stretch = textureStretch1;
            AlphaMaskStretch = alphaStretch;
        }

        public static void Load(ContentManager contentManager)
        {
            Texture2D alphaTexture = contentManager.Load<Texture2D>("land/01000003_noise_alpha");

            Set(LandTileId.Acid, "02000100_Lava_A", 12, "02000490_Acid_A", 8, "02000490_Acid_A", 8); // TO water shader
            Set(LandTileId.Dirt, "02000020_Dirt_A", 4, "02000021_Dirt_B", 1, null, 16);
            Set(LandTileId.Forest, "02000040_Forest_A", 12, "02000041_Forest_B", 8, null, 16);
            Set(LandTileId.Grass, "02000010_Grass_C", 10, "02000011_Grass_B", 4, null, 32);
            Set(LandTileId.Jungle, "02000030_Jungle_A", 5, "02000031_Jungle_B", 4, null, 16);
            Set(LandTileId.Lava, "02000100_Lava_A", 12, "02000100_Lava_A", 4, null, 6); // TO water shader
            Set(LandTileId.Rock, "02000060_Rock_A", 9, "02000061_Rock_B", 6, null, 24);
            Set(LandTileId.Sand, "02000070_Sand_A", 6, "02000071_Sand_B", 6, null, 16);
            Set(LandTileId.Snow, "02000080_Snow_A", 5, "02000081_Snow_B", 5, null, 16);
            Set(LandTileId.Swamp, "02000700_Swamp_Water_A", 4, "02000702_Swamp_Water_C", 3, null, 10); // TO Water shader
            Set(LandTileId.Unused, "02000000_Black_Void_A", 8, "02000001_Black_Void_B", 6, null, 32);
            Set(LandTileId.Water, "02000051_water", 4, "02000051_water", 1, "01000013_water_alpha", 16); // TO Water shader

            void Set(LandTileId id, string texture0, byte stretch0, string texture1, byte stretch1, string? alpha, byte stretchAlpha)
            {
                int index = (int)id;

                Values[index] = new
                (
                    contentManager.Load<Texture2D>($"land/{texture0}"),
                    contentManager.Load<Texture2D>($"land/{texture1}"),
                    alpha is null ? alphaTexture : contentManager.Load<Texture2D>($"land/{alpha}"),
                    stretch0,
                    stretch1,
                    stretchAlpha
                );
            }
        }
    }
}
