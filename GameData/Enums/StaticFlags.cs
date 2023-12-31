﻿using System.Runtime.CompilerServices;

namespace GameData.Enums
{
    public enum StaticFlags : long
    {
        Background = 0x0000000001,
        Weapon = 0x0000000002,
        Transparent = 0x0000000004,
        Translucent = 0x0000000008,
        Wall = 0x0000000010,
        Damaging = 0x0000000020,
        Impassable = 0x0000000040,
        Wet = 0x0000000080,
        Ignored = 0x0000000100,
        Surface = 0x0000000200,
        Bridge = 0x0000000400,
        Generic = 0x0000000800,
        Window = 0x0000001000,
        NoShoot = 0x0000002000,
        ArticleA = 0x0000004000,
        ArticleAn = 0x0000008000,
        Article = 0x000000C000,
        MonGen = 0x000000C000,
        ArticleThe = 0x0000010000,
        Foliage = 0x0000020000,
        PartialHue = 0x0000040000,
        NoHouse = 0x0000080000,
        Map = 0x0000100000,
        Container = 0x0000200000,
        Wearable = 0x0000400000,
        LightSource = 0x0000800000,
        Animation = 0x0001000000,
        HoverOver = 0x0002000000,
        NoDiagonal = 0x0004000000,
        Armor = 0x0008000000,
        Roof = 0x0010000000,
        Door = 0x0020000000,
        StairBack = 0x0040000000,
        StairRight = 0x0080000000,
        AlphaBlend = 0x0100000000,
        UseNewArt = 0x0200000000,
        ArtUsed = 0x0400000000,
        NoShadow = 0x1000000000,
        PixelBleed = 0x2000000000,
        PlayAnimOnce = 0x4000000000,
        MultiMovable = 0x10000000000,
    }

    public static class StaticFlagsExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Has(this StaticFlags flags, StaticFlags flag)
        {
            return (flags & flag) != 0;
        }
    }
}
