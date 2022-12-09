using System.Runtime.InteropServices;

namespace GameData.Structures.Contents.Statics
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly struct StaticTextureInfo
    {
        public readonly int Id;
        public readonly short StartX;
        public readonly short StartY;
        public readonly short EndX;
        public readonly short EndY;
        public readonly short OffsetX;
        public readonly short OffsetY;

        public StaticTextureInfo(int id, short startX, short startY, short endX, short endY, short offsetX, short offsetY)
        {
            Id = id;
            StartX = startX;
            StartY = startY;
            EndX = endX;
            EndY = endY;
            OffsetX = offsetX;
            OffsetY = offsetY;
        }
    }
}
