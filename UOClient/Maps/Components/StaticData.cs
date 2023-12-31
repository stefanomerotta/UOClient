﻿using GameData.Enums;
using System.Runtime.InteropServices;

namespace UOClient.Maps.Components
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly struct StaticData
    {
        public readonly int TextureId;
        public readonly short StartX;
        public readonly short StartY;
        public readonly short EndX;
        public readonly short EndY;
        public readonly short OffsetX;
        public readonly short OffsetY;
        public readonly StaticTileType Type;
        public readonly StaticFlags Flags;
        public readonly bool Enhanced;
        public readonly byte Height;

        public StaticData(int textureId, short startX, short startY, short endX, short endY, 
            short offsetX, short offsetY, StaticTileType type, StaticFlags flags, bool enhanced, byte height)
        {
            TextureId = textureId;
            StartX = startX;
            StartY = startY;
            EndX = endX;
            EndY = endY;
            OffsetX = offsetX;
            OffsetY = offsetY;
            Type = type;
            Flags = flags;
            Enhanced = enhanced;
            Height = height;
        }
    }
}
