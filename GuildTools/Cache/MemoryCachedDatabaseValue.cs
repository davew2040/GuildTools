using GuildTools.EF;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuildTools.Cache
{
    public class MemoryCachedDatabaseValue<T> where T : class
    {
        protected IKeyedResourceManager resourceManager;
        protected IMemoryCache cache;
        protected TimeSpan memoryDuration;

        public MemoryCachedDatabaseValue(
            IMemoryCache cache,
            TimeSpan memoryDuration,
            IKeyedResourceManager resourceManager)
        {
            this.cache = cache;
            this.memoryDuration = memoryDuration;
            this.resourceManager = resourceManager;
        }

        public async Task<T> GetAsync(
            Func<Task<T>> databaseRetriever,
            Func<string> keyCreator)
        {
            var key = keyCreator();
            await this.resourceManager.AcquireKeyLockAsync(key);

            try
            {
                T foundValue;
                var foundInCache = this.cache.TryGetValue(key, out foundValue);

                if (foundInCache)
                {
                    return foundValue;
                }

                var foundInDatabase = await databaseRetriever();
                if (foundInDatabase != null)
                {
                    this.SetMemoryCacheEntry(key, foundInDatabase);
                    return foundInDatabase;
                }

                return null;
            }
            finally
            {
                await this.resourceManager.ReleaseKeyLockAsync(key);
            }
        }

        private void SetMemoryCacheEntry(string key, T newValue)
        {
            var options = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(DateTime.Now + this.memoryDuration);

            this.cache.Set(key, newValue, options);
        }
    }
}