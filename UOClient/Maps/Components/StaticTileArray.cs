namespace UOClient.Maps.Components
{
    internal struct StaticTileArray
    {
        public StaticTile[][] Tiles;
        public int TotalCount;

        public StaticTileArray(StaticTile[][] tiles)
        {
            Tiles = tiles;
            TotalCount = 0;
        }
    }
}
