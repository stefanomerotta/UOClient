//using GameData.Enums;
//using Microsoft.Xna.Framework.Content;
//using Microsoft.Xna.Framework.Graphics;
//using System.Runtime.InteropServices;

//namespace UOClient.Maps.Terrain
//{
//    [StructLayout(LayoutKind.Sequential, Pack = 1)]
//    internal readonly struct LiquidTerrainInfo
//    {
//        public static readonly LiquidTerrainInfo[] Values = new LiquidTerrainInfo[(int)LandTileId.Length];

//        public readonly Texture2D Normal;
//        public readonly Texture2D Texture0;
//        public readonly Texture2D? Texture1;
//        public readonly Texture2D StaticTexture;

//        public readonly byte NormalStretch;
//        public readonly byte Texture0Stretch;
//        public readonly byte Texture1Stretch;
//        public readonly byte StaticTextureStretch;

//        public readonly bool FollowCenter;
//        public readonly float WindSpeed;
//        public readonly float WaveHeight;

//        private LiquidTerrainInfo(Texture2D normal, Texture2D texture0, Texture2D? texture1, Texture2D staticTexture,
//            byte normalStretch, byte texture0Stretch, byte texture1Stretch, byte staticTextureStretch, bool followCenter,
//            float windSpeed, float waveHeight)
//        {
//            Normal = normal;
//            Texture0 = texture0;
//            Texture1 = texture1;
//            StaticTexture = staticTexture;

//            NormalStretch = normalStretch;
//            Texture0Stretch = texture0Stretch;
//            Texture1Stretch = texture1Stretch;
//            StaticTextureStretch = staticTextureStretch;

//            FollowCenter = followCenter;
//            WindSpeed = windSpeed;
//            WaveHeight = waveHeight;
//        }

//        public static void Load(ContentManager contentManager)
//        {
//            Set(LandTileId.Acid, 0.3f, false, 0.01f, "01000012_lava_normal", 12, "02000490_Acid_A", 8, null, 0, "02000490_Acid_A", 8);
//            Set(LandTileId.Lava, 0.2f, false, 0.01f, "01000012_lava_normal", 12, "02000100_Lava_A", 8, "02000101_Lava_B", 8, "02000100_Lava_A", 6);
//            Set(LandTileId.Swamp, 0.3f, true, 0.01f, "02000703_Swamp_Water_D", 16, "02000702_Swamp_Water_C", 20, "02000700_Swamp_Water_A", 4, "02000704_Swamp_Water_E", 10);
//            Set(LandTileId.Water, 0.3f, true, 0, "01000013_water_alpha", 16, "01000017_cube3", 30, null, 0, "02000051_water", 4);

//            void Set(LandTileId id, float waveHeight, bool followCenter, float windSpeed, string normal, byte normalStretch,
//                string texture0, byte stretch0, string? texture1, byte stretch1, string staticTexture, byte staticTextureStretch)
//            {
//                int index = (int)id;

//                Values[index] = new
//                (
//                    contentManager.Load<Texture2D>($"land/{normal}"),
//                    contentManager.Load<Texture2D>($"land/{texture0}"),
//                    texture1 is null ? null : contentManager.Load<Texture2D>($"land/{texture1}"),
//                    contentManager.Load<Texture2D>($"land/{staticTexture}"),
//                    normalStretch,
//                    stretch0,
//                    stretch1,
//                    staticTextureStretch,
//                    followCenter,
//                    windSpeed,
//                    waveHeight
//                );
//            }
//        }
//    }
//}
