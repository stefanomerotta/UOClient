namespace UOClient.Maps.Components
{
    internal readonly struct Bounds
    {
        public readonly int StartX;
        public readonly int StartY;
        public readonly int EndX;
        public readonly int EndY;

        public Bounds(int startX, int startY, int endX, int endY)
        {
            StartX = startX;
            StartY = startY;
            EndX = endX;
            EndY = endY;
        }
    }
}
