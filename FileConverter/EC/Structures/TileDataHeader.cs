using GameData.Enums;
using System.Runtime.InteropServices;

namespace FileConverter.EC.Structures
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct TileDataHeader
    {
        public int DictionaryIndex;
        public int TileId;
        public bool Unk1;
        public bool Unk2;
        public float Unk3;
        public float Unk4;
        public int Unk4b;
        public int OldId;
        public float Unk5;
        public int Type;
        public byte Unk6;
        public int Unk7;
        public int Unk8;
        public int Light1;
        public int Light2;
        public int Unk9;

        public StaticFlags Flags1;
        public long Flags2;
        public int Facing;
    }
}
