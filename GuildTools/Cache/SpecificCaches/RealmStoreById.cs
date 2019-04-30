using GuildTools.Cache.SpecificCaches.CacheInterfaces;
using GuildTools.EF;
using GuildTools.EF.Models;
using GuildTools.EF.Models.Enums;
using GuildTools.EF.Models.StoredBlizzardModels;
using GuildTools.ExternalServices.Blizzard;
using GuildTools.ExternalServices.Blizzard.Utilities;
using GuildTools.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EfEnums = GuildTools.EF.Models.Enums;

namespace GuildTools.Cache.SpecificCaches
{
    public class RealmStoreById : IRealmStoreById
    {
        private readonly TimeSpan MemoryDuration;
        private MemoryCachedDatabaseValue<StoredRealm> cache;
        private IGuildService guildService;
        private GuildToolsContext context;

        public RealmStoreById(IGuildService guildService, IMemoryCache memoryCache, GuildToolsContext context, IKeyedResourceManager resourceManager)
        {
            this.MemoryDuration = TimeSpan.FromDays(1);
            this.guildService = guildService;
            this.context = context;
            this.cache = new MemoryCachedDatabaseValue<StoredRealm>(memoryCache, this.MemoryDuration, resourceManager);
        }

        public async Task<StoredRealm> GetRealmAsync(int id)
        {
            return await this.cache.GetAsync(
                this.GetFromDatabase(id),
                this.GetKey(id));
        }

        private Func<Task<StoredRealm>> GetFromDatabase(int id)
        {
            return (async () =>
            {
                return await this.context.StoredRealms
                    .Include(r => r.Region)
                    .FirstOrDefaultAsync(x => x.Id == id);
            });
        }

        private Func<string> GetKey(int id)
        {
            return () =>
            {
                return Keyifier.GetKey("realmbyid", new List<string>() { id.ToString() });
            };
        }
    }
}
