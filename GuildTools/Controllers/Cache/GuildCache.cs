using GuildTools.ExternalServices;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuildTools.Controllers.Cache
{
    public class GuildCache : IGuildCache
    {
        private HashSet<string> updatingSet;
        private Dictionary<string, ExpiringData<string>> cache;
        private Sql.Data sqlData;
        private const string SqlCacheType = "Guild";
        private IBlizzardService blizzardService;
        private readonly TimeSpan CacheEntryDuration = new TimeSpan(24, 0, 0);

        public GuildCache(IConfiguration configuration, IBlizzardService blizzardService)
        {
            this.cache = new Dictionary<string, ExpiringData<string>>();
            this.blizzardService = blizzardService;
            string connectionString = configuration.GetValue<string>("ConnectionStrings:Database");
            sqlData = new Sql.Data(connectionString);
            this.updatingSet = new HashSet<string>();
        }

        public string Get(string realm, string guild)
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
                    this.cache[key] = new ExpiringData<string>(sqlCachedData.ExpiresOn, sqlCachedData.Value);
                    return sqlCachedData.Value;
                }
            }

            this.Refresh(guild, realm);

            return null;
        }

        public void Refresh(string guild, string realm)
        {
            string key = this.GetKey(realm, guild);

            if (this.updatingSet.Contains(key))
            {
                return;
            }

            this.updatingSet.Add(key);

            Task.Factory.StartNew(async () =>
            {
                var guildData = await this.blizzardService.GetGuildMembers(guild, realm);
                
                this.Add(realm, guild, guildData, CacheEntryDuration);

                this.updatingSet.Remove(key);
            });
        }


        private void Add(string realm, string guild, string value, TimeSpan duration)
        {
            string key = this.GetKey(realm, guild);

            this.cache[key] = new ExpiringData<string>(DateTime.Now + duration, value);
            this.sqlData.SetCachedValue(this.GetKey(realm, guild), value, SqlCacheType, duration);
        }
        
        private string GetKey(string realm, string guild)
        {
            return $"{realm}:{guild}";
        }
    }
}
