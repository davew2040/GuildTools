using GuildTools.Controllers.JsonResponses;
using GuildTools.Data;
using GuildTools.EF;
using GuildTools.ExternalServices.Blizzard;
using GuildTools.Scheduler;
using GuildTools.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static GuildTools.ExternalServices.Blizzard.BlizzardService;

namespace GuildTools.Cache
{
    public class OldGuildMemberCache : IOldGuildMemberCache
    {
        private HashSet<string> updatingSet;
        private Dictionary<string, ExpiringData<IEnumerable<GuildMemberStats>>> cache;
        private IGuildService guildMemberService;
        private const string SqlCacheType = "GuildMember";
        private readonly TimeSpan CacheEntryDuration = new TimeSpan(24, 0, 0);
        private readonly IBackgroundTaskQueue backgroundTaskQueue;
        private readonly IDataRepository dataRepository;

        public OldGuildMemberCache(
            IGuildService guildMemberService,
            IDataRepository dataRepository,
            IBackgroundTaskQueue backgroundTaskQueue)

        {
            this.cache = new Dictionary<string, ExpiringData<IEnumerable<GuildMemberStats>>>();
            this.guildMemberService = guildMemberService;
            this.updatingSet = new HashSet<string>();
            this.dataRepository = dataRepository;
            this.backgroundTaskQueue = backgroundTaskQueue;
        }

        public async Task<IEnumerable<GuildMemberStats>> GetAsync(BlizzardRegion region, string realm, string guild)
        {
            string key = this.GetKey(realm, guild);

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
                var sqlCachedData = await this.dataRepository.GetCachedValueAsync(key);

                if (null != sqlCachedData)
                {
                    var deserializedData = JsonConvert.DeserializeObject<IEnumerable<GuildMemberStats>>(sqlCachedData.Value);
                    this.cache[key] = new ExpiringData<IEnumerable<GuildMemberStats>>(sqlCachedData.ExpiresOn, deserializedData);
                    return deserializedData;
                }
            }

            if (!this.updatingSet.Contains(key))
            {
                this.backgroundTaskQueue.QueueBackgroundWorkItem(async (token, serviceProvider) =>
                {
                    await this.Refresh(region, guild, realm, serviceProvider);
                });
            }

            return null;
        }

        public async Task Refresh(BlizzardRegion region, string guild, string realm, IServiceProvider serviceProvider)
        {
            string key = this.GetKey(realm, guild);

            if (this.updatingSet.Contains(key))
            {
                return;
            }

            this.updatingSet.Add(key);

            using (var serviceScope = serviceProvider.CreateScope())
            {
                var repo = serviceScope.ServiceProvider.GetService<IDataRepository>();

                await repo.SetCachedValueAsync("test_key", "test_value", TimeSpan.FromSeconds(10));
            }

            var guildPlayers = await this.guildMemberService.GetLargeGuildMembersDataAsync(region, guild, realm);

            using (var serviceScope = serviceProvider.CreateScope())
            {
                var repo = serviceScope.ServiceProvider.GetService<IDataRepository>();

                await this.AddAsync(realm, guild, guildPlayers, CacheEntryDuration, repo);
            }


            this.updatingSet.Remove(key);
        }

        private async Task AddAsync(string realm, string guild, IEnumerable<GuildMemberStats> value, TimeSpan duration, IDataRepository dataRepo)
        {
            string key = this.GetKey(realm, guild);

            this.cache[key] = new ExpiringData<IEnumerable<GuildMemberStats>>(DateTime.Now + duration, value);
            await dataRepo.SetCachedValueAsync(this.GetKey(realm, guild), JsonConvert.SerializeObject(value), duration);
        }

        private string GetKey(string realm, string guild)
        {
            realm = BlizzardService.FormatRealmName(realm);
            guild = BlizzardService.FormatRealmName(guild);

            return $"{realm}:{guild}";
        }
    }
}
