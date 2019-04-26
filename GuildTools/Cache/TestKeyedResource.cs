using GuildTools.ExternalServices;
using GuildTools.ExternalServices.Blizzard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuildTools.Cache
{
    public class TestKeyedResource
    {
        private IKeyedResourceManager resourceManager;
        private DatabaseCacheWithMemoryCache<string> cache;

        private const string keyPrefix = "members";

        public TestKeyedResource(IKeyedResourceManager resourceManager, DatabaseCacheWithMemoryCache<string> cache)
        {
            this.cache = cache;
            this.resourceManager = resourceManager;
        }

        public async Task<string> Get(string guild, string realm, BlizzardService.BlizzardRegion region)
        {
            string key = this.getKey(guild, realm, region);

            string value;
            await this.resourceManager.AcquireKeyLockAsync(key);

            var existingValue = await this.cache.TryGetValueAsync(key);
            if (existingValue != null)
            {
                await this.resourceManager.ReleaseKeyLockAsync(key);

                return existingValue;
            }

            var retrievedValue = await this.GetResource(guild, realm, region);

            await this.cache.InsertValueAsync(key, retrievedValue);

            await this.resourceManager.ReleaseKeyLockAsync(key);

            return retrievedValue;
        }

        private async Task<string> GetResource(string guild, string realm, BlizzardService.BlizzardRegion region)
        {
            await Task.Delay(2000);

            return this.getKey(guild, realm, region) + "_" + DateTime.Now.ToString();
        }

        private string getKey(string guild, string realm, BlizzardService.BlizzardRegion region)
        {
            return $"{keyPrefix}:{guild}:{realm}:{region.ToString()}";
        }
    }
}
