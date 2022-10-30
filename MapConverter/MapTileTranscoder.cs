using GameData.Enums;
using System.Diagnostics;

namespace MapConverter
{
    internal class MapTileTranscoder
    {
        private static readonly LandTileId[] ids = new LandTileId[ushort.MaxValue];

        public static LandTileId GetNewId(ushort oldId)
        {
            return ids[oldId];
        }

        private static void Set(LandTileId newId, params ushort[] oldIds)
        {
            for (int i = 0; i < oldIds.Length; i++)
            {
                Debug.Assert(ids[oldIds[i]] == 0);

                ids[oldIds[i]] = newId;
            }
        }

        static MapTileTranscoder()
        {
            Set(LandTileId.Dirt, 113, 114, 115, 116, 117, 118, 119, 120);
            Set(LandTileId.Forest, 196, 197, 198, 199);
            Set(LandTileId.Furrows, 9, 14, 336, 341);
            Set(LandTileId.Jungle, 172, 173, 174, 175);
            Set(LandTileId.Lava, 500, 501, 502, 503);

            Set(LandTileId.Rock, 556, 557, 558, 559, 1754, 1755, 1756, 1757,
                1787, 1788, 1789, 1790, 1851, 1852, 1853, 1854, 1881, 1882, 1883, 1884,
                2001, 2002, 2003, 2004);

            Set(LandTileId.Sand, 22, 23, 24, 25);
            Set(LandTileId.Snow, 282, 283, 284, 285);
            Set(LandTileId.Unused, 0, 1, 2);
            Set(LandTileId.Water, 168, 169, 170, 171, 310, 311);
            Set(LandTileId.Swamp, 15849, 15850, 15851, 15852);

            Set(LandTileId.Grass, 3, 4, 5, 6);

            Set(LandTileId.Acid, 11790, 11791, 11818, 11819,
                11820, 11821, 11822, 11823, 11824, 11825, 11826, 11827, 11828, 11829,
                11830, 11831, 11832, 11833, 11834, 11835);

            // Sand cliffs
            Set(LandTileId.Sand, 26, 27, 442, 443, 444, 445, 446, 447, 448, 449,
                450, 451, 452, 453, 454, 455, 456, 457, 458, 459,
                460, 461, 462, 463, 464, 465);

            // Sand cliffs -> grass
            Set(LandTileId.Sand, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39,
                40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 68, 69, 70, 71, 72, 73, 74, 75);

            // Forest <-> grass
            Set(LandTileId.Grass, 192, 193, 194, 195, 
                200, 201, 202, 203, 204, 205, 206, 207, 208, 209,
                210, 211, 212, 213, 214, 215, 216, 217, 218, 219);
        }
    }
}
