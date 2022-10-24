using System.Runtime.InteropServices;
using TexturePacker.Components;

namespace TexturePacker
{
    internal static unsafe class CRuntime
    {
        private static readonly int rectSize = sizeof(Rect);

        public static void* Malloc(long size)
        {
            IntPtr ptr = Marshal.AllocHGlobal((int)size);
            return ptr.ToPointer();
        }

        public static void Free(void* a)
        {
            IntPtr ptr = new(a);
            Marshal.FreeHGlobal(ptr);
        }

        public static void QSort(Rect* data, int count, delegate*<Rect*, Rect*, int> comparer)
        {
            QSortInternal(data, comparer, 0, count - 1);
        }

        private static void QSortInternal(Rect* data, delegate*<Rect*, Rect*, int> comparer, int left, int right)
        {
            if (left < right)
            {
                int p = QSortPartition(data, comparer, left, right);

                QSortInternal(data, comparer, left, p);
                QSortInternal(data, comparer, p + 1, right);
            }
        }

        private static int QSortPartition(Rect* data, delegate*<Rect*, Rect*, int> comparer, int left, int right)
        {
            Rect* pivot = data + rectSize * left;
            int i = left - 1;
            int j = right + 1;

            while (true)
            {
                do
                {
                    i++;
                }
                while (comparer(data + rectSize * i, pivot) < 0);

                do
                {
                    j--;
                }
                while (comparer(data + rectSize * j, pivot) > 0);

                if (i >= j)
                    return j;

                QSortSwap(data, i, j);
            }
        }

        private static void QSortSwap(Rect* data, int pos1, int pos2)
        {
            Rect* a = data + rectSize * pos1;
            Rect* b = data + rectSize * pos2;

            for (int k = 0; k < rectSize; k++)
            {
                (*b, *a) = (*a, *b);
                a++;
                b++;
            }
        }
    }
}