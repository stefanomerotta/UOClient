using Microsoft.Extensions.ObjectPool;

namespace UOClient.Utilities
{
    public static class Utility
    {
        public static ObjectPool<T> CreatePool<T>(int maxRetained)
            where T : class, new()
        {
            DefaultObjectPoolProvider provider = new() { MaximumRetained = maxRetained };
            return provider.Create<T>();
        }

        public static ObjectPool<T> CreatePool<T>(int maxRetained, IPooledObjectPolicy<T> policy)
            where T : class
        {
            DefaultObjectPoolProvider provider = new() { MaximumRetained = maxRetained };
            return provider.Create(policy);
        }
    }
}
