//using GameData.Enums;
//using Microsoft.Xna.Framework.Content;
//using Microsoft.Xna.Framework.Graphics;
//using System.Runtime.InteropServices;

//namespace UOClient.Maps.Terrain
//{
//    [StructLayout(LayoutKind.Sequential, Pack = 1)]
//    internal readonly struct SolidTerrainInfo
//    {
//        public static readonly SolidTerrainInfo[] Values = new SolidTerrainInfo[(int)LandTileId.Length];

//        public readonly Texture2D Texture0;
//        public readonly Texture2D Texture1;
//        public readonly Texture2D AlphaMask;

//        public readonly byte Texture0Stretch;
//        public readonly byte Texture1Stretch;
//        public readonly byte AlphaMaskStretch;

//        private SolidTerrainInfo(Texture2D texture0, Texture2D texture1, Texture2D alphaMask,
//            byte textureStretch0, byte textureStretch1, byte alphaMaskStretch)
//        {
//            Texture0 = texture0;
//            Texture1 = texture1;
//            AlphaMask = alphaMask;

//            Texture0Stretch = textureStretch0;
//            Texture1Stretch = textureStretch1;
//            AlphaMaskStretch = alphaMaskStretch;
//        }

//        public static void Load(ContentManager contentManager)
//        {
//            Texture2D alphaTexture = contentManager.Load<Texture2D>("land/01000003_noise_alpha");

//            Set(LandTileId.Dirt, "02000020_Dirt_A", 4, "02000021_Dirt_B", 1, null, 16);
//            Set(LandTileId.Forest, "02000040_Forest_A", 12, "02000041_Forest_B", 8, null, 16);
//            Set(LandTileId.Grass, "02000010_Grass_C", 10, "02000011_Grass_B", 4, null, 32);
//            Set(LandTileId.Jungle, "02000030_Jungle_A", 5, "02000031_Jungle_B", 4, null, 16);
//            Set(LandTileId.Rock, "02000060_Rock_A", 9, "02000061_Rock_B", 6, null, 24);
//            Set(LandTileId.Sand, "02000070_Sand_A", 6, "02000071_Sand_B", 6, null, 16);
//            Set(LandTileId.Snow, "02000080_Snow_A", 5, "02000081_Snow_B", 5, null, 16);
//            Set(LandTileId.Unused, "02000000_Black_Void_A", 8, "02000001_Black_Void_B", 6, null, 32);
//            Set(LandTileId.SandCliff_E_W, "02000540_Sand_Cliff_EW_A", 1, "02000541_Sand_Cliff_EW_B", 1, null, 8);
//            Set(LandTileId.SandCliff_NW_SE, "02000550_Sand_Cliff_NWtoSE_A", 2, "02000551_Sand_Cliff_NWtoSE_B", 2, null, 8);
//            Set(LandTileId.SandCliff_N_S, "02000560_Sand_Cliff_NS_A", 1, "02000561_Sand_Cliff_NS_B", 1, null, 8);
//            Set(LandTileId.SandCliff_NE_SW, "02000570_Sand_Cliff_NEtoSW_A", 1, "02000571_Sand_Cliff_NEtoSW_B", 1, null, 8);

//            void Set(LandTileId id, string texture0, byte stretch0, string texture1, byte stretch1, string? alpha, byte stretchAlpha)
//            {
//                int index = (int)id;

//                Values[index] = new
//                (
//                    contentManager.Load<Texture2D>($"land/{texture0}"),
//                    contentManager.Load<Texture2D>($"land/{texture1}"),
//                    alpha is null ? alphaTexture : contentManager.Load<Texture2D>($"land/{alpha}"),
//                    stretch0,
//                    stretch1,
//                    stretchAlpha
//                );
//            }
//        }
//    }
//}
