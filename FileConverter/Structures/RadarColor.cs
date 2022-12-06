﻿using System.Runtime.InteropServices;

namespace FileConverter.Structures
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct RadarColor
    {
        public byte R;
        public byte G;
        public byte B;
        public byte A;
    }
}