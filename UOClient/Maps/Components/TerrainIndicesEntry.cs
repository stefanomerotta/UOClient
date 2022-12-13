namespace UOClient.Maps.Components
{
    public readonly struct TerrainIndicesEntry
    {
        public readonly int TileId;
        public readonly short[] Indices;

        public TerrainIndicesEntry(int tileId, short[] indices)
        {
            TileId = tileId;
            Indices = indices;
        }
    }
}
