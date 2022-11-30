using System.Runtime.InteropServices;

namespace Common.Utilities
{
    public static class SpanExtensions
    {
        public static Span<TTo> Cast<TFrom, TTo>(this Span<TFrom> instance)
            where TFrom : struct
            where TTo : struct
        {
            return MemoryMarshal.Cast<TFrom, TTo>(instance);
        }

        public static ReadOnlySpan<T> AsReadOnlySpan<T>(this T[] instance)
        {
            return new(instance);
        }
    }
}
