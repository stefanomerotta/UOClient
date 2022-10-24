using TexturePacker.Components;

namespace TexturePacker
{
    public readonly struct PackedRectangle
    {
        public readonly int X;
        public readonly int Y;
        public readonly int Width;
        public readonly int Height;

        internal PackedRectangle(Rect rect)
        {
            X = rect.X;
            Y = rect.Y;
            Width = rect.W;
            Height = rect.H;
        }
    }
}
