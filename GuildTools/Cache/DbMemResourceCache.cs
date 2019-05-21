using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuildTools.Cache
{
    public class DbMemCachedResource<T>
    {
        private TimeSpan expiresAfter;
        protected DatabaseCacheWithMemoryCache cache;
        protected IKeyedResourceManager resourceManager;

        public DbMemCachedResource(TimeSpan expiresAfter, IDatabaseCache databaseCache, IMemoryCache memoryCache, IKeyedResourceManager resourceManager)
        {
            this.expiresAfter = expiresAfter;
            this.cache = new DatabaseCacheWithMemoryCache(expiresAfter, databaseCache, memoryCache);
            this.resourceManager = resourceManager;
        }

        public async Task<T> GetOrCacheAsync(Func<Task<T>> dataRetriever, Func<string> keyCreator)
        {
            var key = keyCreator();
            await this.resourceManager.AcquireKeyLockAsync(key);

            try
            {
                var foundValue = await this.cache.TryGetValueAsync<T>(key);

                if (foundValue.Found)
                {
                    return foundValue.Result;
                }

                var newValue = await dataRetriever();

                await this.cache.InsertValueAsync(key, newValue, this.expiresAfter);

                return newValue;
            }
            finally
            {
                await this.resourceManager.ReleaseKeyLockAsync(key);
            }
        }
    }
}
