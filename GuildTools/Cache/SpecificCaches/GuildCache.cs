using GuildTools.Cache.SpecificCaches.CacheInterfaces;
using GuildTools.Controllers.JsonResponses;
using GuildTools.Controllers.Models;
using GuildTools.EF.Models.Enums;
using GuildTools.ExternalServices;
using GuildTools.ExternalServices.Blizzard;
using GuildTools.ExternalServices.Blizzard.JsonParsing;
using GuildTools.ExternalServices.Blizzard.Utilities;
using GuildTools.Services;
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
    public class GuildCache : IGuildCache
    {
        private DbMemCachedResource<GuildSlim> cache;
        private IGuildService guildService;

        public GuildCache(IGuildService guildService, IMemoryCache memoryCache, IDatabaseCache dbCache, IKeyedResourceManager resourceManager)
        {
            this.cache = new DbMemCachedResource<GuildSlim>(TimeSpan.FromDays(1.0), dbCache, memoryCache, resourceManager);
            this.guildService = guildService;
        }

        public async Task<GuildSlim> GetGuild(GameRegionEnum region, string guildName, string realmName)
        {
            var locatedGuild = await this.cache.GetOrCacheAsync(
                async () =>
                {
                    var result = await this.guildService.GetGuild(BlizzardUtilities.GetBlizzardRegionFromEfRegion(region), realmName, guildName);

                    return result;
                },
                () => this.GetKey(region, guildName, realmName));

            return locatedGuild;
        }

        private string GetKey(GameRegionEnum region, string guildName, string realmName)
        {
            var realmKey = Keyifier.GetRealmKey(realmName);
            var regionKey = Keyifier.GetRegionKey(region);
            var guildKey = Keyifier.GetGuildKey(guildName);

            return Keyifier.GetKey("guild", new List<string>() { regionKey, realmKey, guildKey });
        }
    }
}
