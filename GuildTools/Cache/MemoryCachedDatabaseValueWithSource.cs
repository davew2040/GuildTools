using GuildTools.EF;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuildTools.Cache
{
    public class MemoryCachedDatabaseValueWithSource<T> where T : class
    {
        protected IKeyedResourceManager resourceManager;

        protected IMemoryCache cache;
        protected TimeSpan memoryDuration;

        public MemoryCachedDatabaseValueWithSource(
            IMemoryCache cache,
            TimeSpan memoryDuration,
            IKeyedResourceManager resourceManager)
        {
            this.cache = cache;
            this.memoryDuration = memoryDuration;
            this.resourceManager = resourceManager;
        }

        public async Task<T> GetOrCacheAsync(
            Func<Task<T>> databaseRetriever,
            Func<Task<CacheResult<T>>> sourceRetriever,
            Func<T, Task> databaseStorer,
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

                var foundFromSource = await sourceRetriever();
                if (!foundFromSource.Found)
                {
                    this.SetMemoryCacheEntry(key, null);
                    return null;
                }

                await databaseStorer(foundFromSource.Result);
                var retrieved = await databaseRetriever();
                this.SetMemoryCacheEntry(key, retrieved);

                return foundFromSource.Result;
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