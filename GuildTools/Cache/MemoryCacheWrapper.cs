using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuildTools.Cache
{
    public class MemoryCacheWrapper : ICache
    {
        private IMemoryCache memoryCache;

        public MemoryCacheWrapper(IMemoryCache memoryCache)
        {
            this.memoryCache = memoryCache;
        }

        public async Task<CacheResult<T>> TryGetValueAsync<T>(string key)
        {
            T cachedValue;

            if (this.memoryCache.TryGetValue<T>(key, out cachedValue))
            {
                return new CacheResult<T>()
                {
                    Found = true,
                    Result = cachedValue
                };
            }

            return new CacheResult<T>()
            {
                Found = false
            };
        }

        public async Task InsertValueAsync<T>(string key, T newValue, TimeSpan expiresAfter)
        {
            this.memoryCache.Set(key, newValue, DateTime.Now + expiresAfter);
        }

        public async Task RemoveAsync(string key)
        {
            this.memoryCache.Remove(key);
        }
    }
}
