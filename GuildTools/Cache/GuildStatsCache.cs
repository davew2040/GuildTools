using GuildTools.Data;
using GuildTools.ExternalServices;
using GuildTools.ExternalServices.Blizzard;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using static GuildTools.ExternalServices.Blizzard.BlizzardService;
using GuildTools.EF;

namespace GuildTools.Cache
{
    public class GuildStatsCache : IGuildStatsCache
    {
        private HashSet<string> updatingSet;
        private Dictionary<string, ExpiringData<string>> cache;
        private const string SqlCacheType = "Guild";
        private readonly IBlizzardService blizzardService;
        private readonly TimeSpan CacheEntryDuration = new TimeSpan(24, 0, 0);
        private readonly IDataRepository dataRepo;
        private readonly IServiceProvider serviceProvider;

        public GuildStatsCache(IConfiguration configuration, IBlizzardService blizzardService, IDataRepository dataRepo, IServiceProvider serviceProvider)
        {
            this.cache = new Dictionary<string, ExpiringData<string>>();
            this.blizzardService = blizzardService;
            string connectionString = configuration.GetValue<string>("ConnectionStrings:Database");
            this.updatingSet = new HashSet<string>();
            this.dataRepo = dataRepo;
            this.serviceProvider = serviceProvider;
        }

        public async Task<string> Get(BlizzardRegion region, string realm, string guild)
        {
            string key = this.GetKey(region, realm, guild);

            if (this.cache.ContainsKey(key))
            {
                var expiringValue = this.cache[key];
                if (expiringValue.IsExpired)
                {
                    this.cache.Remove(key);
                }
                else
                {
                    return this.cache[key].Data;
                }
            }
            else
            {
                var sqlCachedData = await this.dataRepo.GetCachedValueAsync(key);

                if (null != sqlCachedData)
                {
                    this.cache[key] = new ExpiringData<string>(sqlCachedData.ExpiresOn, sqlCachedData.Value);
                    return sqlCachedData.Value;
                }
            }

            this.Refresh(region, guild, realm);

            return null;
        }

        public void Refresh(BlizzardRegion region, string guild, string realm)
        {
            string key = this.GetKey(region, realm, guild);

            if (this.updatingSet.Contains(key))
            {
                return;
            }

            this.updatingSet.Add(key);

            Task.Factory.StartNew(async () =>
            {

                using (IDataRepository dataRepo = this.serviceProvider.GetService<IDataRepository>())
                {
                    await dataRepo.SetCachedValueAsync("test_key", "test_value", TimeSpan.FromSeconds(10));
                }

                var guildData = await this.blizzardService.GetGuildMembersAsync(guild, realm, region);

                using (IDataRepository dataRepo = this.serviceProvider.GetService<IDataRepository>())
                {
                    await this.Add(region, realm, guild, guildData, CacheEntryDuration, dataRepo);
                }

                this.updatingSet.Remove(key);
            });
        }

        private async Task Add(BlizzardRegion region, string realm, string guild, string value, TimeSpan duration, IDataRepository repo)
        {
            string key = this.GetKey(region, realm, guild);

            this.cache[key] = new ExpiringData<string>(DateTime.Now + duration, value);

            await repo.SetCachedValueAsync(key, value, duration);
        }
        
        private string GetKey(BlizzardRegion region, string realm, string guild)
        {
            return $"{BlizzardService.GetRegionString(region)}:{realm}:{guild}";
        }
    }
}
