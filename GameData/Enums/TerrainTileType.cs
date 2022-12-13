namespace GameData.Enums
{
    [Flags]
    public enum TerrainTileType : byte
    {
        None = 0x0,
        Solid = 0x1,
        Liquid = 0x2,
        Smooth = 0x4,
        FollowCenter = 0x8,
        Single = 0x10
    }

    public static class TerrainTileTypeExtensions
    {
        public static bool Has(this TerrainTileType instance, TerrainTileType flag)
        {
            return (instance & flag) != 0;
        }
    }
}
