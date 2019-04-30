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
    public class GuildMemberCache : IGuildMemberCache
    {
        private DbMemCachedResource<IEnumerable<BlizzardPlayer>> cache;
        private IGuildService guildService;

        public GuildMemberCache(IGuildService guildService, IMemoryCache memoryCache, IDatabaseCache dbCache, IKeyedResourceManager resourceManager)
        {
            this.cache = new DbMemCachedResource<IEnumerable<BlizzardPlayer>>(TimeSpan.FromHours(1.0), TimeSpan.FromDays(1.0), dbCache, memoryCache, resourceManager);
            this.guildService = guildService;
        }

        public async Task<IEnumerable<BlizzardPlayer>> GetMembers(GameRegion region, string realmName, string guildName)
        {
            return await this.cache.GetOrCacheAsync(async () =>
            {
                var result = await this.guildService.GetSlimGuildMembersDataAsync(BlizzardUtilities.GetBlizzardRegionFromEfRegion(region), guildName, realmName);

                return result;
            },
            () => this.GetKey(region, guildName, realmName));
        }

        private string GetKey(GameRegion region, string guildName, string realmName)
        {
            var realmKey = Keyifier.GetRealmKey(realmName);
            var regionKey = Keyifier.GetRegionKey(region);
            var guildKey = Keyifier.GetGuildKey(guildName);

            return Keyifier.GetKey("guildmembers", new List<string>() { regionKey, realmKey, guildKey });
        }
    }
}
