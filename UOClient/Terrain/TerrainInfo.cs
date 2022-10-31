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
        public readonly Texture2D Texture2;
        public readonly Texture2D? Texture3;

        public readonly byte Texture0Stretch;
        public readonly byte Texture1Stretch;
        public readonly byte Texture2Stretch;
        public readonly byte Texture3Stretch;

        private TerrainInfo(Texture2D texture0, Texture2D texture1, Texture2D texture2, Texture2D? texture3,
            byte textureStretch0, byte textureStretch1, byte texture2Stretch, byte texture3Stretch)
        {
            Texture0 = texture0;
            Texture1 = texture1;
            Texture2 = texture2;
            Texture3 = texture3;

            Texture0Stretch = textureStretch0;
            Texture1Stretch = textureStretch1;
            Texture2Stretch = texture2Stretch;
            Texture3Stretch = texture3Stretch;
        }

        private TerrainInfo(Texture2D texture0, Texture2D texture1, Texture2D alphaMask,
            byte textureStretch0, byte textureStretch1, byte alphaMaskStretch)
        {
            Texture0 = texture0;
            Texture1 = texture1;
            Texture2 = alphaMask;
            Texture3 = null;

            Texture0Stretch = textureStretch0;
            Texture1Stretch = textureStretch1;
            Texture2Stretch = alphaMaskStretch;
            Texture3Stretch = 0;
        }

        public static void Load(ContentManager contentManager)
        {
            LoadSolid(contentManager);
            LoadLiquid(contentManager);
        }

        private static void LoadSolid(ContentManager contentManager)
        {
            Texture2D alphaTexture = contentManager.Load<Texture2D>("land/01000003_noise_alpha");

            //Set(SolidLandTileId.Acid, "02000100_Lava_A", 12, "02000490_Acid_A", 8, "02000490_Acid_A", 8); // TO water shader
            Set(LandTileId.Dirt, "02000020_Dirt_A", 4, "02000021_Dirt_B", 1, null, 16);
            Set(LandTileId.Forest, "02000040_Forest_A", 12, "02000041_Forest_B", 8, null, 16);
            Set(LandTileId.Grass, "02000010_Grass_C", 10, "02000011_Grass_B", 4, null, 32);
            Set(LandTileId.Jungle, "02000030_Jungle_A", 5, "02000031_Jungle_B", 4, null, 16);
            //Set(SolidLandTileId.Lava, "02000100_Lava_A", 12, "02000100_Lava_A", 4, null, 6); // TO water shader
            Set(LandTileId.Rock, "02000060_Rock_A", 9, "02000061_Rock_B", 6, null, 24);
            Set(LandTileId.Sand, "02000070_Sand_A", 6, "02000071_Sand_B", 6, null, 16);
            Set(LandTileId.Snow, "02000080_Snow_A", 5, "02000081_Snow_B", 5, null, 16);
            //Set(SolidLandTileId.Swamp, "02000700_Swamp_Water_A", 4, "02000702_Swamp_Water_C", 3, null, 10); // TO Water shader
            Set(LandTileId.Unused, "02000000_Black_Void_A", 8, "02000001_Black_Void_B", 6, null, 32);
            //Set(SolidLandTileId.Water, "02000051_water", 4, "02000051_water", 1, "01000013_water_alpha", 16); // TO Water shader
            Set(LandTileId.SandCliff_E_W, "02000540_Sand_Cliff_EW_A", 1, "02000541_Sand_Cliff_EW_B", 1, null, 8);
            Set(LandTileId.SandCliff_NW_SE, "02000550_Sand_Cliff_NWtoSE_A", 2, "02000551_Sand_Cliff_NWtoSE_B", 2, null, 8);
            Set(LandTileId.SandCliff_N_S, "02000560_Sand_Cliff_NS_A", 1, "02000561_Sand_Cliff_NS_B", 1, null, 8);
            Set(LandTileId.SandCliff_NE_SW, "02000570_Sand_Cliff_NEtoSW_A", 1, "02000571_Sand_Cliff_NEtoSW_B", 1, null, 8);

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

        private static void LoadLiquid(ContentManager contentManager)
        {
            Set(LandTileId.Acid, "01000012_lava_normal", 12, "02000490_Acid_A", 8, "02000490_Acid_A", 8);
            Set(LandTileId.Lava, "01000012_lava_normal", 12, "02000100_Lava_A", 4, "02000100_Lava_A", 6, "02000101_Lava_B", 8);
            Set(LandTileId.Swamp, "02000703_Swamp_Water_D", 16, "02000700_Swamp_Water_A", 4, "02000702_Swamp_Water_C", 3, "02000704_Swamp_Water_E", 10);
            Set(LandTileId.Water, "02000051_water", 4, "02000051_water", 1, "01000013_water_alpha", 16);

            void Set(LandTileId id, string normal, byte normalStretch,
                string texture1, byte stretch1, string texture2, byte stretch2, string? texture3 = null, byte stretch3 = 0)
            {
                int index = (int)id;

                Values[index] = new
                (
                    contentManager.Load<Texture2D>($"land/{normal}"),
                    contentManager.Load<Texture2D>($"land/{texture1}"),
                    contentManager.Load<Texture2D>($"land/{texture2}"),
                    texture3 is null ? null : contentManager.Load<Texture2D>($"land/{texture3}"),
                    normalStretch,
                    stretch1,
                    stretch2,
                    stretch3
                );
            }
        }
    }
}
