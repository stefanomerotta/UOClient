using GameData.Enums;
using System.Runtime.InteropServices;

namespace GameData.Structures.Contents.Terrains
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly struct LiquidTerrainData
    {
        public readonly ushort Id;
        public readonly float Speed;
        public readonly float WaveHeight;
        public readonly TerrainTileType Type;
        public readonly TerrainTextureInfo Normal;
        public readonly TerrainTextureInfo Texture0;
        public readonly TerrainTextureInfo Static;

        public LiquidTerrainData(ushort id, TerrainTileType type, float speed, float waveHeight, TerrainTextureInfo normal,
            TerrainTextureInfo texture0, TerrainTextureInfo @static)
        {
            Id = id;
            Type = type;
            Speed = speed;
            WaveHeight = waveHeight;
            Normal = normal;
            Texture0 = texture0;
            Static = @static;
        }
    }
}
