using System.Collections.Generic;

namespace Rucker.Extensions
{
    public static class DictionaryExtensions
    {
        public static V GetOrDefault<K, V>(this IDictionary<K, V> dictionary, K key)
        {
            return key != null && dictionary.ContainsKey(key) ? dictionary[key] : default(V);
        }
    }
}