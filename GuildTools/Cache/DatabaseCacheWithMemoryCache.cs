using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuildTools.Cache
{
    public class DatabaseCacheWithMemoryCache : ICache
    {
        private TimeSpan expiresAfter;
        private IMemoryCache memoryCache;
        private IDatabaseCache dbCache;

        public DatabaseCacheWithMemoryCache(TimeSpan expiresAfter, IDatabaseCache databaseCache, IMemoryCache memoryCache)
        {
            this.memoryCache = memoryCache;
            this.dbCache = databaseCache;
        }

        public async Task<CacheResult<T>> TryGetValueAsync<T>(string key)
        {
            T tryValue;

            if (this.memoryCache.TryGetValue(key, out tryValue))
            {
                return new CacheResult<T>()
                {
                    Found = true,
                    Result = tryValue
                };
            }

            var dbResult = await this.dbCache.TryGetValueAsync<T>(key);

            if (dbResult.Found)
            {
                this.SetMemoryCacheEntry(key, tryValue, this.expiresAfter);
                return dbResult;
            }

            return new CacheResult<T>()
            {
                Found = false
            };
        }

        public async Task InsertValueAsync<T>(string key, T newValue, TimeSpan expiresAfter)
        {
            this.SetMemoryCacheEntry(key, newValue, expiresAfter);

            await this.dbCache.InsertValueAsync(key, newValue, expiresAfter);
        }

        public async Task RemoveAsync(string key)
        {
            this.memoryCache.Remove(key);
            await this.dbCache.RemoveAsync(key);
        }

        private void SetMemoryCacheEntry<T>(string key, T newValue, TimeSpan expiresAfter)
        {
            var options = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(DateTime.Now + expiresAfter);

            this.memoryCache.Set(key, newValue, options);
        }
    }
}
