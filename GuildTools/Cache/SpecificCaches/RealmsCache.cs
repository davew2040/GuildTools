using GuildTools.Cache.SpecificCaches.CacheInterfaces;
using GuildTools.Controllers.JsonResponses;
using GuildTools.Controllers.Models;
using GuildTools.EF.Models.Enums;
using GuildTools.ExternalServices;
using GuildTools.ExternalServices.Blizzard;
using GuildTools.ExternalServices.Blizzard.JsonParsing;
using GuildTools.ExternalServices.Blizzard.Utilities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static GuildTools.ExternalServices.Blizzard.BlizzardService;
using EfEnums = GuildTools.EF.Models.Enums;

namespace GuildTools.Cache.SpecificCaches
{
    public class RealmsCache : IRealmsCache
    {
        private DbMemCachedResource<IEnumerable<Realm>> cache;
        private IBlizzardService blizzardService;

        public RealmsCache(IBlizzardService blizzardService, IMemoryCache memoryCache, IDatabaseCache dbCache, IKeyedResourceManager resourceManager)
        {
            this.cache = new DbMemCachedResource<IEnumerable<Realm>>(TimeSpan.FromHours(1.0), TimeSpan.FromDays(1.0), dbCache, memoryCache, resourceManager);
            this.blizzardService = blizzardService;
        }

        public async Task<IEnumerable<Realm>> GetRealms(EfEnums.GameRegion region)
        {
            return await this.cache.GetOrCacheAsync(async () =>
            {
                var json = await this.blizzardService.GetRealmsByRegionAsync(BlizzardUtilities.GetBlizzardRegionFromEfRegion(region));

                return RealmParsing.GetRealms(json);
            },
            () => this.GetKey(region));
        }

        private string GetKey(GameRegion region)
        {
            var regionKey = Keyifier.GetRegionKey(region);

            return Keyifier.GetKey("realm", new List<string>() { regionKey });
        }
    }
}
