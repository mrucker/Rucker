using System;

namespace Data.Core
{
    public interface ICache
    {
        object Get(string key);
        void Set(string key, object value);
    }

    public static class ICacheExtensions
    {
        public static T Get<T>(this ICache cache, string key, Func<T> getter)
        {
            if (getter == null) throw new ArgumentNullException(nameof(getter));

            var cachedValue = cache.Get(key);

            if (cachedValue is T)
            {
                return (T)cachedValue;
            }

            var storedValue = getter();

            cache.Set(key, storedValue);

            return storedValue;
        }
    }
}