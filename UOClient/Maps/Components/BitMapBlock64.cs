namespace UOClient.Maps.Components
{
    public unsafe ref struct BitMapBlock64
    {
        public fixed ulong flags[64];

        public ushort TrueCount { get; private set; }

        public bool this[int x, int y]
        {
            get => (flags[y] & 0x1UL << x) != 0;
            set => Set(x, y, value);
        }

        private void Set(int x, int y, bool value)
        {
            ref ulong pointer = ref flags[y];

            if ((pointer & 0x1UL << x) != 0 == value)
                return;

            if (value)
            {
                pointer |= 0x1UL << x;
                TrueCount++;
            }
            else
            {
                pointer &= ~(0x1UL << x);
                TrueCount--;
            }
        }
    }
}
