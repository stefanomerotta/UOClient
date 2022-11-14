namespace UOClient.ECS.Events
{
    internal readonly struct CurrentSectorChanged
    {
        public readonly ushort X;
        public readonly ushort Y;

        public CurrentSectorChanged(ushort x, ushort y)
        {
            X = x;
            Y = y;
        }
    }
}
