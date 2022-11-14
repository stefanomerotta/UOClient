using System;

namespace UOClient.Utilities.SingleThreaded
{
    internal sealed class DisposableObjectPool<T> : IDisposable where T : class, IDisposable
    {
        private readonly T?[] pool;
        private readonly Action<T>? onReturned;
        private int count;
        private bool disposed;

        public DisposableObjectPool(int maxSize, Action<T>? onReturned = null)
        {
            pool = new T[maxSize];
            this.onReturned = onReturned;
        }

        public DisposableObjectPool(T[] objects, Action<T>? onReturned = null)
        {
            pool = objects;
            count = objects.Length;
            this.onReturned = onReturned;
        }

        public bool TryGet(out T? obj)
        {
            if (disposed || count == 0)
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

            if (disposed)
            {
                obj.Dispose();
                return;
            }

            if (count == pool.Length)
                return;

            pool[count++] = obj;
        }

        public void Dispose()
        {
            if (disposed)
                return;

            disposed = true;

            for (int i = 0; i < count; i++)
            {
                pool[i]!.Dispose();
                pool[i] = null;
            }

            count = 0;
        }
    }
}
