using System.Runtime.InteropServices;

namespace GameData.Structures.Contents.Statics
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly struct StaticProperties
    {
        public readonly byte Height;

        public StaticProperties(byte height)
        {
            Height = height;
        }
    }
}
