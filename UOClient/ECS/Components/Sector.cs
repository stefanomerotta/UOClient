namespace UOClient.ECS.Components
{
    internal struct Sector
    {
        public ushort X;
        public ushort Y;

        public Sector(int x, int y)
        {
            X = (ushort)x;
            Y = (ushort)y;
        }

        public bool OutOfRange(int x, int y, int range)
        {
            int deltaX = X - x;
            int deltaY = Y - y;

            return deltaX < -range || deltaX > range || deltaY < -range || deltaY > range;
        }
    }
}
