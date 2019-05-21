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
    public class PlayerCache : IPlayerCache
    {
        private DbMemCachedResource<BlizzardPlayer> cache;
        private IGuildService guildService;

        public PlayerCache(IGuildService guildService, IMemoryCache memoryCache, IDatabaseCache dbCache, IKeyedResourceManager resourceManager)
        {
            this.cache = new DbMemCachedResource<BlizzardPlayer>(TimeSpan.FromDays(1.0), dbCache, memoryCache, resourceManager);
            this.guildService = guildService;
        }

        public async Task<BlizzardPlayer> GetPlayer(GameRegionEnum region, string playerName, string realmName)
        {
            return await this.cache.GetOrCacheAsync(async () =>
            {
                var result = await this.guildService.GetSinglePlayerAsync(BlizzardUtilities.GetBlizzardRegionFromEfRegion(region), realmName, playerName);

                return result;
            },
            () => this.GetKey(region, playerName, realmName));
        }

        private string GetKey(GameRegionEnum region, string playerName, string realmName)
        {
            var realmKey = Keyifier.GetRealmKey(realmName);
            var regionKey = Keyifier.GetRegionKey(region);
            var guildKey = Keyifier.GetPlayerKey(playerName);

            return Keyifier.GetKey("player", new List<string>() { regionKey, realmKey, guildKey });
        }
    }
}
