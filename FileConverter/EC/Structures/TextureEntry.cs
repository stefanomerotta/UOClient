using GameData.Enums;
using System.Runtime.InteropServices;

namespace FileConverter.EC.Structures
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TextureEntry
    {
        public int DictionaryIndex;
        public float TextureStretch;
    }
}
