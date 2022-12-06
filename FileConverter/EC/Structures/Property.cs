using System.Runtime.InteropServices;

namespace FileConverter.EC.Structures
{
    internal enum PropertyKey : byte
    {
        Weight = 0,
        Quality = 1,
        Quantity = 2,
        Height = 3,
        Value = 4,
        AcVc = 5,
        Slot = 6,
        off_C8 = 7,
        Appearance = 8,
        Race = 9,
        Gender = 10,
        Paperdoll = 11
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct Property
    {
        public PropertyKey Key;
        public int Value;
    }
}
