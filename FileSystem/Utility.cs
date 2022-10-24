namespace FileSystem
{
    public static class Utility
    {
        public static uint CalculateHash(ReadOnlySpan<byte> d)
        {
            uint a = 1;
            uint b = 0;

            for (int i = 0; i < d.Length; i++)
            {
                a = (a + d[i]) % 0xFFF1;
                b = (b + a) % 0xFFF1;
            }

            return (b << 16) | a;
        }
    }
}
