using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuildTools.Cache
{
    public class TestKeyedResource
    {
        private IKeyedResourceManager resourceManager;
        DatabaseCacheWithMemoryCache<string> cache;

        public TestKeyedResource(IKeyedResourceManager resourceManager, DatabaseCacheWithMemoryCache<string> cache)
        {
            this.cache = cache;
            this.resourceManager = resourceManager;
        }

        public async Task<string> Get(string key)
        {
            string value;
            await this.resourceManager.AcquireKeyLockAsync(key);

            var existingValue = await this.cache.TryGetValueAsync(key);
            if (existingValue != null)
            {
                await this.resourceManager.ReleaseKeyLockAsync(key);

                return existingValue;
            }

            var retrievedValue = await this.GetResource(key);

            await this.cache.InsertValueAsync(key, retrievedValue);

            await this.resourceManager.ReleaseKeyLockAsync(key);

            return retrievedValue;
        }

        private async Task<string> GetResource(string key)
        {
            await Task.Delay(10000);

            return key + "_value = " + DateTime.Now.ToString();
        }
    }
}
