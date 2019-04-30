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
    public class RealmStoreByValues : IRealmStoreByValues
    {
        private readonly TimeSpan MemoryDuration;
        private MemoryCachedDatabaseValueWithSource<StoredRealm> cache;
        private IGuildService guildService;
        private GuildToolsContext context;

        public RealmStoreByValues(IGuildService guildService, IMemoryCache memoryCache, GuildToolsContext context, IKeyedResourceManager resourceManager)
        {
            this.MemoryDuration = TimeSpan.FromDays(1);
            this.guildService = guildService;
            this.context = context;
            this.cache = new MemoryCachedDatabaseValueWithSource<StoredRealm>(memoryCache, this.MemoryDuration, resourceManager);
        }

        public async Task<StoredRealm> GetRealmAsync(string name, EfEnums.GameRegion region)
        {
            return await this.cache.GetOrCacheAsync(
                this.GetFromDatabase(name, region),
                this.GetFromSource(name, region),
                this.Store(),
                this.GetKey(name, region));
        }

        private Func<Task<StoredRealm>> GetFromDatabase(string name, EfEnums.GameRegion region)
        {
            return (async () =>
            {
                return await this.context.StoredRealms
                    .Include(r => r.Region)
                    .SingleOrDefaultAsync(x => x.Slug == BlizzardService.BuildRealmSlug(name) 
                        && x.RegionId == (int)region);
            });
        }

        private Func<Task<CacheResult<StoredRealm>>> GetFromSource(string name, EfEnums.GameRegion region)
        {
            return (async () =>
            {
                var result = await this.guildService.GetRealmAsync(name, BlizzardUtilities.GetBlizzardRegionFromEfRegion(region));

                if (result == null)
                {
                    return new CacheResult<StoredRealm>()
                    {
                        Found = false
                    };
                }

                return new CacheResult<StoredRealm>()
                {
                    Found = true,
                    Result = new StoredRealm()
                    {
                        RegionId = (int)region,
                        Name = result.Name,
                        Slug = result.Slug
                    }
                };
            });
        }

        private Func<StoredRealm, Task> Store()
        {
            return async (realm) =>
            {
                this.context.StoredRealms.Add(realm);

                await this.context.SaveChangesAsync();
            };
        }

        private Func<string> GetKey(string name, EfEnums.GameRegion region)
        {
            return () =>
            {
                var realmKey = Keyifier.GetGuildKey(name);
                var regionKey = Keyifier.GetRegionKey(region);

                return Keyifier.GetKey("realmbyvalues", new List<string>() { realmKey, regionKey });
            };
        }
    }
}
