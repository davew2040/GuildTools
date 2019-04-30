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
    public class GuildStoreByName : IGuildStoreByName
    {
        private readonly TimeSpan MemoryDuration;
        private MemoryCachedDatabaseValueWithSource<StoredGuild> cache;
        private IBlizzardService blizzardService;
        private GuildToolsContext context;

        public GuildStoreByName(IBlizzardService blizzardService, IMemoryCache memoryCache, GuildToolsContext context, IKeyedResourceManager resourceManager)
        {
            this.MemoryDuration = TimeSpan.FromDays(1);
            this.blizzardService = blizzardService;
            this.context = context;
            this.cache = new MemoryCachedDatabaseValueWithSource<StoredGuild>(memoryCache, this.MemoryDuration, resourceManager);
        }

        public async Task<StoredGuild> GetGuildAsync(string guildName, StoredRealm realm, int profileId)
        {
            return await this.cache.GetOrCacheAsync(
                this.GetFromDatabase(guildName, profileId, realm),
                this.GetFromSource(guildName, profileId, realm),
                this.Store(),
                this.GetKey(guildName, profileId, realm.Id));
        }

        private Func<Task<StoredGuild>> GetFromDatabase(string guildName, int profileId, StoredRealm realm)
        {
            return (async () =>
            {
                return await this.context.StoredGuilds.SingleOrDefaultAsync(
                    x => x.Name == guildName 
                        && x.ProfileId == profileId
                        && x.RealmId == realm.Id);
            });
        }

        private Func<Task<CacheResult<StoredGuild>>> GetFromSource(string guildName, int profileId, StoredRealm realm)
        {
            return (async () =>
            {
                var json = await this.blizzardService.GetGuildAsync(
                    guildName, 
                    realm.Name, 
                    BlizzardUtilities.GetBlizzardRegionFromEfRegion((EfEnums.GameRegion)realm.Region.Id));

                if (BlizzardService.DidGetFail(json))
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
                        RealmId = realm.Id
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

        private Func<string> GetKey(string guildName, int profileId, int realmId)
        {
            return () =>
            {
                var guildKey = Keyifier.GetGuildKey(guildName);

                return Keyifier.GetKey("guildstore", new List<string>() { guildKey, profileId.ToString(), realmId.ToString() });
            };
        }
    }
}
