using GameData.Structures.Contents.Statics;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace UOClient.Maps.Components
{
    internal readonly struct StaticTileList
    {
        private readonly byte[] rawData;
        public readonly int TotalStaticsCount;

        public StaticTileList(byte[] rawData, int totalStaticsCount)
        {
            this.rawData = rawData;
            TotalStaticsCount = totalStaticsCount;
        }

        public StaticTileListEnumerator GetEnumerator() => new(rawData);

        public ref struct StaticTileListEnumerator
        {
            private readonly byte[] rawData;
            private int index;

            public ReadOnlySpan<StaticTile> Current { get; private set; }

            public StaticTileListEnumerator(byte[] rawData)
            {
                this.rawData = rawData;
            }

            public bool MoveNext()
            {
                if (index >= rawData.Length)
                    return false;

                int size = rawData[index] * Unsafe.SizeOf<StaticTile>();

                if (size == 0)
                    Current = Span<StaticTile>.Empty;
                else
                    Current = MemoryMarshal.Cast<byte, StaticTile>(rawData.AsSpan(index + 1, size));

                index += size + 1;

                return true;
            }

            public void Reset()
            {
                index = 0;
                Current = default;
            }
        }
    }
}
