using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuildTools.Cache
{
    public class DbMemCachedResource<T>
    {
        protected DatabaseCacheWithMemoryCache<T> cache;
        protected IKeyedResourceManager resourceManager;

        public DbMemCachedResource(TimeSpan memoryDuration, TimeSpan dbDuration, IDatabaseCache databaseCache, IMemoryCache memoryCache, IKeyedResourceManager resourceManager)
        {
            this.cache = new DatabaseCacheWithMemoryCache<T>(memoryDuration, dbDuration, databaseCache, memoryCache);
            this.resourceManager = resourceManager;
        }

        public async Task<T> GetOrCacheAsync(Func<Task<T>> dataRetriever, Func<string> keyCreator)
        {
            var key = keyCreator();
            await this.resourceManager.AcquireKeyLockAsync(key);

            try
            {
                var foundValue = await this.cache.TryGetValueAsync(key);

                if (foundValue.Found)
                {
                    return foundValue.Result;
                }

                var newValue = await dataRetriever();

                await this.cache.InsertValueAsync(key, newValue);

                return newValue;
            }
            finally
            {
                await this.resourceManager.ReleaseKeyLockAsync(key);
            }
        }
    }
}
