using System.Collections.Generic;

namespace Data.Core
{
    public static class DictionaryExtensions
    {
        public static V GetOrDefault<K, V>(this IDictionary<K, V> dictionary, K key)
        {
            return key != null && dictionary.ContainsKey(key) ? dictionary[key] : default(V);
        }
    }
}