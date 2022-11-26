using System.Runtime.CompilerServices;

namespace Common.Utilities
{
    internal static class UnsafeUtility
    {
        public static ref TTo As<TFrom, TTo>(in TFrom @from)
        {
            ref TFrom m = ref Unsafe.AsRef(in @from);
            return ref Unsafe.As<TFrom, TTo>(ref m);
        }
    }
}
