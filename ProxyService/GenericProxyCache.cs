using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ProxyService
{
    class GenericProxyCache<T>
    {
        ObjectCache cache = MemoryCache.Default;

        public T Get(string cacheItemName)
        {
            if (cache.Contains(cacheItemName))
            {
                return (T)cache[cacheItemName];
            }

            return default(T);
        }

        public T Get(string cacheItemName, double dt_seconds)
        {
            var dtOffset = DateTimeOffset.Now.AddSeconds(dt_seconds);
            return Get(cacheItemName, dtOffset);
        }

        public T Get(string cacheItemName, DateTimeOffset dt)
        {
            if (cache.Contains(cacheItemName))
            {
                return (T)cache[cacheItemName];
            }

            return default(T);
        }

    }
}
