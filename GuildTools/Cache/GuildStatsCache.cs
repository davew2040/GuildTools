using GuildTools.Data;
using GuildTools.ExternalServices;
using GuildTools.ExternalServices.Blizzard;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static GuildTools.ExternalServices.Blizzard.BlizzardService;

namespace GuildTools.Cache
{
    public class GuildStatsCache : IGuildStatsCache
    {
        private HashSet<string> updatingSet;
        private Dictionary<string, ExpiringData<string>> cache;
        private const string SqlCacheType = "Guild";
        private IBlizzardService blizzardService;
        private readonly TimeSpan CacheEntryDuration = new TimeSpan(24, 0, 0);
        private readonly IDataRepository dataRepo;


        public GuildStatsCache(IConfiguration configuration, IBlizzardService blizzardService, IDataRepository dataRepo)
        {
            this.cache = new Dictionary<string, ExpiringData<string>>();
            this.blizzardService = blizzardService;
            string connectionString = configuration.GetValue<string>("ConnectionStrings:Database");
            this.updatingSet = new HashSet<string>();
            this.dataRepo = dataRepo;
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
                var guildData = await this.blizzardService.GetGuildMembersAsync(guild, realm, region);
                
                await this.Add(region, realm, guild, guildData, CacheEntryDuration);

                this.updatingSet.Remove(key);
            });
        }


        private async Task Add(BlizzardRegion region, string realm, string guild, string value, TimeSpan duration)
        {
            string key = this.GetKey(region, realm, guild);

            this.cache[key] = new ExpiringData<string>(DateTime.Now + duration, value);

            await this.dataRepo.SetCachedValueAsync(key, value, duration);
        }
        
        private string GetKey(BlizzardRegion region, string realm, string guild)
        {
            return $"{BlizzardService.GetRegionString(region)}:{realm}:{guild}";
        }
    }
}
