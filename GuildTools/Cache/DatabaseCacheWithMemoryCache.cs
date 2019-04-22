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

            if (!EqualityComparer<T>.Default.Equals(tryValue, default(T)))
            {
                return tryValue;
            }

            tryValue = await this.dbCache.TryGetValueAsync(key);

            return tryValue;
        }

        public override async Task InsertValueAsync(string key, T newValue)
        {
            var entry = this.memoryCache.CreateEntry(key).SetValue(newValue).SetAbsoluteExpiration(DateTime.Now + this.memoryDuration);

            this.memoryCache.Set(key, entry);

            await this.dbCache.InsertValueAsync(key, newValue);
        }
    }
}
