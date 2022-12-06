using System;
using System.Runtime.InteropServices;

namespace UOClient.Utilities
{
    internal static class SpanExtensions
    {
        public static Span<TTo> Cast<TFrom, TTo>(this Span<TFrom> instance) 
            where TFrom : struct
            where TTo : struct
        {
            return MemoryMarshal.Cast<TFrom, TTo>(instance);
        }
    }
}
