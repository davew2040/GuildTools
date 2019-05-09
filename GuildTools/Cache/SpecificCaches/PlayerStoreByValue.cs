using GuildTools.Cache.SpecificCaches.CacheInterfaces;
using GuildTools.Controllers.JsonResponses;
using GuildTools.EF;
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
using GuildTools.EF.Models.StoredBlizzardModels;
using EfEnums = GuildTools.EF.Models.Enums;
using EfModels = GuildTools.EF.Models;
using Microsoft.EntityFrameworkCore;

namespace GuildTools.Cache.SpecificCaches
{
    public class PlayerStoreByValue : IPlayerStoreByValue
    {
        private readonly TimeSpan MemoryDuration;
        private MemoryCachedDatabaseValueWithSource<StoredPlayer> cache;
        private IGuildService guildService;
        private GuildToolsContext context;

        public PlayerStoreByValue(
            IGuildService guildService, 
            IMemoryCache memoryCache,
            GuildToolsContext context,
            IKeyedResourceManager resourceManager)
        {
            this.guildService = guildService;
            this.context = context;
            this.cache = new MemoryCachedDatabaseValueWithSource<StoredPlayer>(memoryCache, this.MemoryDuration, resourceManager);
        }

        public async Task<StoredPlayer> GetPlayerAsync(string playerName, StoredRealm realm, StoredGuild guild, int profileId)
        {
            return await this.cache.GetOrCacheAsync(
                this.GetFromDatabase(playerName, realm.Id, profileId),
                this.GetFromSource(playerName, realm, guild, profileId),
                this.Store(),
                this.GetKey(playerName, realm.Name, profileId));
        }

        public async Task InsertPlayerAsync(StoredPlayer player, int profileId)
        {
            await this.cache.InsertIfMissingAsync(
                player,
                this.GetFromDatabase(player.Name, player.RealmId, player.ProfileId),
                this.Store(),
                this.GetKey(player.Name, player.Realm.Name, player.ProfileId));
        }

        public Task<StoredPlayer> GetPlayersAsync(IEnumerable<StoredPlayer> players)
        {
            throw new NotImplementedException();
        }

        private Func<Task<StoredPlayer>> GetFromDatabase(string name, int realmId, int profileId)
        {
            return (async () =>
            {
                return await this.context.StoredPlayers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Name == name && x.RealmId == realmId && x.ProfileId == profileId);
            });
        }

        private Func<Task<CacheResult<StoredPlayer>>> GetFromSource(string name, StoredRealm realm, StoredGuild guild, int profileId)
        {
            return (async () =>
            {
                var result = await this.guildService.GetSinglePlayerAsync(
                    BlizzardUtilities.GetBlizzardRegionFromEfRegion((GameRegionEnum)realm.Region.Id),
                    realm.Slug,
                    name);

                if (result == null)
                {
                    return new CacheResult<StoredPlayer>()
                    {
                        Found = false
                    };
                }

                return new CacheResult<StoredPlayer>()
                {
                    Found = true,
                    Result = new StoredPlayer()
                    {
                        Name = result.PlayerName,
                        RealmId = realm.Id,
                        ProfileId = profileId,
                        GuildId = guild.Id,
                        Level = result.Level,
                        Class = result.Class
                    }
                };
            });
        }

        private Func<StoredPlayer, Task<StoredPlayer>> Store()
        {
            return async (player) =>
            {
                this.context.StoredPlayers.Add(player);

                await this.context.SaveChangesAsync();

                return player;
            };
        }

        private Func<string> GetKey(string playerName, string realmName, int profileId)
        {
            return () =>
            {
                var playerKey = Keyifier.GetPlayerKey(playerName);
                var realmKey = Keyifier.GetRealmKey(realmName);

                return Keyifier.GetKey("playerbyvalues", new List<string>() { playerName.ToLower(), realmKey, profileId.ToString() });
            };
        }
    }
}
