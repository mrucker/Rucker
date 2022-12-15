using System.Collections.Generic;

namespace Data.Core
{
    public class DictionaryCache: ICache
    {
        private readonly Dictionary<string, object> _cache; 

        public DictionaryCache()
        {
            _cache = new Dictionary<string, object>();
        }

        public object Get(string key)
        {
            return _cache.ContainsKey(key) ? _cache[key] : null;
        }

        public void Set(string key, object value)
        {
            _cache.Remove(key);
            
            if (value != null)
            {
                _cache.Add(key, value);
            }
        }
    }
}