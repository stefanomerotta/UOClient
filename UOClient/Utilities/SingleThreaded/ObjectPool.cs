using System;
using System.Diagnostics.CodeAnalysis;

namespace UOClient.Utilities.SingleThreaded
{
    internal sealed class ObjectPool<T> where T : class
    {
        private readonly T[] pool;
        private readonly Action<T>? onReturned;
        private int count;

        public ObjectPool(int maxSize, Action<T>? onReturned = null)
        {
            pool = new T[maxSize];
            this.onReturned = onReturned;
        }

        public ObjectPool(T[] objects, Action<T>? onReturned = null)
        {
            pool = objects;
            count = objects.Length;
            this.onReturned = onReturned;
        }

        public bool TryGet([NotNullWhen(true)] out T? obj)
        {
            if (count == 0)
            {
                obj = null;
                return false;
            }

            obj = pool[--count];
            return true;
        }

        public void Return(T obj)
        {
            onReturned?.Invoke(obj);

            if (count == pool.Length)
                return;

            pool[count++] = obj;
        }
    }
}
