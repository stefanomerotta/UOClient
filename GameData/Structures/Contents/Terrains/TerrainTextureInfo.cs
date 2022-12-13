using System.Runtime.InteropServices;

namespace GameData.Structures.Contents.Terrains
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly struct TerrainTextureInfo
    {
        public readonly int Id;
        public readonly float Stretch;

        public TerrainTextureInfo(int id, float stretch)
        {
            Id = id;
            Stretch = stretch;
        }
    }
}
