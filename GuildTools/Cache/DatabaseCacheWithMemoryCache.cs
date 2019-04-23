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
        private IMemoryCache memoryCache;
        private DatabaseCache<T> dbCache;

        public DatabaseCacheWithMemoryCache(TimeSpan memoryDuration, DatabaseCache<T> databaseCache, IMemoryCache memoryCache)
        {
            this.memoryDuration = memoryDuration;
            this.memoryCache = memoryCache;
            this.dbCache = databaseCache;
        }

        public override async Task<T> TryGetValueAsync(string key)
        {
            T tryValue;

            this.memoryCache.TryGetValue(key, out tryValue);

            if (tryValue != null)
            {
                return tryValue;
            }

            tryValue = await this.dbCache.TryGetValueAsync(key);

            if (!EqualityComparer<T>.Default.Equals(tryValue, default(T)))
            {
                this.SetMemoryCacheEntry(key, tryValue);
            }

            return tryValue;
        }

        public override async Task InsertValueAsync(string key, T newValue)
        {
            this.SetMemoryCacheEntry(key, newValue);

            await this.dbCache.InsertValueAsync(key, newValue);
        }

        public void SetMemoryCacheEntry(string key, T newValue)
        {
            var options = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(DateTime.Now + this.memoryDuration);

            this.memoryCache.Set(key, newValue, options);
        }
    }
}
