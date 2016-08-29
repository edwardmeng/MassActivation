using System.Collections.Generic;

namespace MassActivation.Services
{
    public class MemoryCacheService : ICacheService
    {
        private readonly IDictionary<string,object> _cache = new Dictionary<string, object>();
        public object Get(string key)
        {
            object value;
            return _cache.TryGetValue(key, out value) ? value : null;
        }

        public void Set(string key, object value)
        {
            _cache[key] = value;
        }
    }
}
