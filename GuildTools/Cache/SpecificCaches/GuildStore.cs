using GuildTools.EF;
using GuildTools.EF.Models;
using GuildTools.EF.Models.Enums;
using GuildTools.EF.Models.StoredBlizzardModels;
using GuildTools.ExternalServices.Blizzard;
using GuildTools.ExternalServices.Blizzard.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EfEnums = GuildTools.EF.Models.Enums;

namespace GuildTools.Cache.SpecificCaches
{
    public class GuildStore
    {
        private readonly TimeSpan MemoryDuration;
        private MemoryCachedDatabaseValue<StoredGuild> cache;
        private IBlizzardService blizzardService;
        private GuildToolsContext context;

        public GuildStore(IBlizzardService blizzardService, IMemoryCache memoryCache, GuildToolsContext context, IKeyedResourceManager resourceManager)
        {
            this.MemoryDuration = TimeSpan.FromDays(1);
            this.blizzardService = blizzardService;
            this.context = context;
            this.cache = new MemoryCachedDatabaseValue<StoredGuild>(memoryCache, this.MemoryDuration, resourceManager);
        }

        public async Task<StoredGuild> Get(string name, int profileId, int realmId)
        {
            return await this.cache.GetOrCacheAsync(
                this.GetFromDatabase(name, profileId, realmId),
                this.GetFromSource(name, profileId, realmId),
                this.Store(),
                this.GetKey(name, profileId, realmId));
        }

        private Func<Task<StoredGuild>> GetFromDatabase(string name, int profileId, int realmId)
        {
            return (async () =>
            {
                return await this.context.StoredGuilds.SingleOrDefaultAsync(
                    x => x.Name == name 
                        && x.ProfileId == profileId
                        && x.RealmId == realmId);
            });
        }

        private Func<Task<CacheResult<StoredGuild>>> GetFromSource(string name, int profileId, int realmId)
        {
            //FIXME
            realmId = 42;
            EfEnums.GameRegion region = EfEnums.GameRegion.US;
            return (async () =>
            {
                var json = await this.blizzardService.GetGuildAsync(
                    name, 
                    "Burning Blade", 
                    BlizzardUtilities.GetBlizzardRegionFromEfRegion((EfEnums.GameRegion)region));

                if (BlizzardService.DidGetGuildFail(json))
                {
                    return null;
                }

                var guild = ExternalServices.Blizzard.JsonParsing.GuildParsing.GetGuild(json);

                return new CacheResult<StoredGuild>() {
                    Found = true,
                    Result = new StoredGuild()
                    {
                        Name = guild.Name,
                        ProfileId = profileId,
                        RealmId = realmId
                    }};
            });
        }

        private Func<StoredGuild, Task> Store()
        {
            return async (guild) =>
            {
                this.context.StoredGuilds.Add(guild);

                await this.context.SaveChangesAsync();
            };
        }

        private Func<string> GetKey(string name, int profileId, int realmId)
        {
            return () =>
            {
                var guildKey = Keyifier.GetGuildKey(name);

                return $"guild_{guildKey}_{profileId}_{realmId}";
            };
        }
    }
}
