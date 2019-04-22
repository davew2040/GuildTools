using GuildTools.Configuration;
using GuildTools.Controllers.JsonResponses;
using GuildTools.ExternalServices;
using GuildTools.JsonParsing;
using GuildTools.Scheduler;
using GuildTools.Services;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using static GuildTools.ExternalServices.BlizzardService;

namespace GuildTools.Cache
{
    public class GuildMemberCache : IGuildMemberCache
    {
        private HashSet<string> updatingSet;
        private Dictionary<string, ExpiringData<IEnumerable<GuildMember>>> cache;
        private IGuildMemberService guildMemberService;
        private Sql.Data sqlData;
        private const string SqlCacheType = "GuildMember";
        private readonly TimeSpan CacheEntryDuration = new TimeSpan(24, 0, 0);
        private readonly IBackgroundTaskQueue backgroundQueue;

        public GuildMemberCache(
            IConfiguration configuration, 
            IGuildMemberService guildMemberService,
            IBackgroundTaskQueue backgroundQueue)
        {
            this.cache = new Dictionary<string, ExpiringData<IEnumerable<GuildMember>>>();
            this.guildMemberService = guildMemberService;
            string connectionString = configuration.GetValue<string>(ConfigurationKeys.DatabaseConnection);
            sqlData = new Sql.Data(connectionString);
            this.updatingSet = new HashSet<string>();
            this.backgroundQueue = backgroundQueue;
        }

        public IEnumerable<GuildMember> Get(Region region, string realm, string guild)
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
                var sqlCachedData = this.sqlData.GetCachedValue(key, SqlCacheType);

                if (null != sqlCachedData)
                {
                    var deserializedData = JsonConvert.DeserializeObject<IEnumerable<GuildMember>>(sqlCachedData.Value);
                    this.cache[key] = new ExpiringData<IEnumerable<GuildMember>>(sqlCachedData.ExpiresOn, deserializedData);
                    return deserializedData;
                }
            }

            if (!this.updatingSet.Contains(key))
            {
                this.backgroundQueue.QueueBackgroundWorkItem(async token =>
                {
                    await this.Refresh(region, guild, realm);
                });
            }

            return null;
        }

        public async Task Refresh(Region region, string guild, string realm)
        {
            string key = this.GetKey(realm, guild);

            if (this.updatingSet.Contains(key))
            {
                return;
            }

            this.updatingSet.Add(key);

            var guildPlayers = await this.guildMemberService.GetGuildMemberDataAsync(region, guild, realm);

            this.Add(realm, guild, guildPlayers, CacheEntryDuration);

            this.updatingSet.Remove(key);
        }

        private void Add(string realm, string guild, IEnumerable<GuildMember> value, TimeSpan duration)
        {
            string key = this.GetKey(realm, guild);

            this.cache[key] = new ExpiringData<IEnumerable<GuildMember>>(DateTime.Now + duration, value);
            this.sqlData.SetCachedValue(this.GetKey(realm, guild), JsonConvert.SerializeObject(value), SqlCacheType, duration);
        }

        private string GetKey(string realm, string guild)
        {
            realm = BlizzardService.FormatRealmName(realm);
            guild = BlizzardService.FormatRealmName(guild);

            return $"{realm}:{guild}";
        }
    }
}
