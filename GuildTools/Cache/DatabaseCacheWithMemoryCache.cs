using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuildTools.Cache
{
    public class DatabaseCacheWithMemoryCache<T> : Cache<T>
    {
        private TimeSpan memoryDuration;
        private TimeSpan dbDuration;
        private IMemoryCache memoryCache;
        private IDatabaseCache dbCache;

        public DatabaseCacheWithMemoryCache(TimeSpan memoryDuration, TimeSpan dbDuration, IDatabaseCache databaseCache, IMemoryCache memoryCache)
        {
            this.memoryDuration = memoryDuration;
            this.dbDuration = dbDuration;
            this.memoryCache = memoryCache;
            this.dbCache = databaseCache;
        }

        public override async Task<CacheResult<T>> TryGetValueAsync(string key)
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
                this.SetMemoryCacheEntry(key, tryValue);
                return dbResult;
            }

            return new CacheResult<T>()
            {
                Found = false
            };
        }

        public override async Task InsertValueAsync(string key, T newValue)
        {
            this.SetMemoryCacheEntry(key, newValue);

            await this.dbCache.InsertValueAsync(key, newValue, this.dbDuration);
        }

        public void SetMemoryCacheEntry(string key, T newValue)
        {
            var options = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(DateTime.Now + this.memoryDuration);

            this.memoryCache.Set(key, newValue, options);
        }
    }
}
