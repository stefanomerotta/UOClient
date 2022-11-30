using System.Runtime.CompilerServices;

namespace UOClient.Utilities
{
    internal static class UnsafeUtility
    {
        public static ref readonly TTo As<TFrom, TTo>(in TFrom @from) 
        {
            ref TFrom m = ref Unsafe.AsRef(in @from);
            return ref Unsafe.As<TFrom, TTo>(ref m);
        }
    }
}
