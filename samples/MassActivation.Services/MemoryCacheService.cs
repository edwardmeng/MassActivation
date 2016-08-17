using System.Collections;

namespace MassActivation.Services
{
    public class MemoryCacheService : ICacheService
    {
        private readonly Hashtable _cache = new Hashtable();
        public object Get(string key)
        {
            return _cache[key];
        }

        public void Set(string key, object value)
        {
            _cache[key] = value;
        }
    }
}
