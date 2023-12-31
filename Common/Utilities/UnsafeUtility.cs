﻿using System.Runtime.CompilerServices;

namespace Common.Utilities
{
    public static class UnsafeUtility
    {
        public static ref readonly TTo As<TFrom, TTo>(in TFrom @from)
        {
            ref TFrom m = ref Unsafe.AsRef(in @from);
            return ref Unsafe.As<TFrom, TTo>(ref m);
        }
    }
}
