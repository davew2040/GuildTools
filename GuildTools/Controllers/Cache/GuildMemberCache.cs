using GuildTools.Controllers.JsonResponses;
using GuildTools.ExternalServices;
using GuildTools.Scheduler;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace GuildTools.Controllers.Cache
{
    public class GuildMemberCache : IGuildMemberCache
    {
        private HashSet<string> updatingSet;
        private Dictionary<string, ExpiringData<IEnumerable<GuildMember>>> cache;
        private Sql.Data sqlData;
        private const string SqlCacheType = "GuildMember";
        private IBlizzardService blizzardService;
        private readonly TimeSpan CacheEntryDuration = new TimeSpan(24, 0, 0);
        private readonly IBackgroundTaskQueue backgroundQueue;
        private readonly TimeSpan FilterPlayersOlderThan = new TimeSpan(90, 0, 0, 0);

        public GuildMemberCache(IConfiguration configuration, IBlizzardService blizzardService, IBackgroundTaskQueue backgroundQueue)
        {
            this.cache = new Dictionary<string, ExpiringData<IEnumerable<GuildMember>>>();
            this.blizzardService = blizzardService;
            string connectionString = configuration.GetValue<string>("ConnectionStrings:Database");
            sqlData = new Sql.Data(connectionString);
            this.updatingSet = new HashSet<string>();
            this.backgroundQueue = backgroundQueue;
        }

        public IEnumerable<GuildMember> Get(string realm, string guild)
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
                    await this.Refresh(guild, realm);
                });
            }

            return null;
        }

        public async Task Refresh(string guild, string realm)
        {
            string key = this.GetKey(realm, guild);

            if (this.updatingSet.Contains(key))
            {
                return;
            }

            this.updatingSet.Add(key);

            var guildPlayers = await this.GetGuildMemberData(guild, realm);

            this.Add(realm, guild, guildPlayers, CacheEntryDuration);

            this.updatingSet.Remove(key);
        }

        private async Task<IEnumerable<GuildMember>> GetGuildMemberData(string guild, string realm)
         {
            var guildData = await this.blizzardService.GetGuildMembers(guild, realm);

            JObject parsedGuild = JsonConvert.DeserializeObject((string)guildData) as JObject;
            var members = this.GetPlayersJsonFromGuild(parsedGuild).ToList();

            List<GuildMember> validMembers = new List<GuildMember>();

            foreach (var member in members)
            {
                try
                {
                    if (await this.PopulateMemberData(member))
                    {
                        validMembers.Add(member);
                    }
                }
                catch 
                {
                    //Swallow any errors
                }
            }

            return validMembers;
        }

        private async Task<bool> PopulateMemberData(GuildMember member)
        {
            var itemsTask = this.blizzardService.GetPlayerItems(member.Realm, member.Name);
            var mountsTask = this.blizzardService.GetPlayerMounts(member.Realm, member.Name);
            var petsTask = this.blizzardService.GetPlayerPets(member.Realm, member.Name);
            var pvpTask = this.blizzardService.GetPlayerPvpStats(member.Realm, member.Name);

            await Task.WhenAll(new Task[] { itemsTask, mountsTask, petsTask, pvpTask });

            try
            {
                // Let's take this opportunity to filter out inactive players.
                var itemsJObject = JsonConvert.DeserializeObject(itemsTask.Result) as JObject;

                var lastModified = itemsJObject.SelectToken("lastModified").ToString();
                var lastModifiedDateTime = BlizzardService.FromUnixTime(long.Parse(lastModified));
                if (lastModifiedDateTime < DateTime.Now - FilterPlayersOlderThan)
                {
                    return false;
                }

                this.PopulateItemsDetailsFromJson(member, itemsJObject);
            }
            catch { Debug.WriteLine("Error reading player items for " + member.Name); }

            try
            {
                var mountsJObject = JsonConvert.DeserializeObject(mountsTask.Result) as JObject;
                this.PopulateMountCountFromJson(member, mountsJObject);
            }
            catch { Debug.WriteLine("Error reading mounts for " + member.Name); }

            try
            {
                var petsJObject = JsonConvert.DeserializeObject(petsTask.Result) as JObject;
                this.PopulatePetCountFromJson(member, petsJObject);
            }
            catch { Debug.WriteLine("Error reading pets for " + member.Name); }

            try
            {
                var pvpJObject = JsonConvert.DeserializeObject(pvpTask.Result) as JObject;
                this.PopulatePvpStatsFromJson(member, pvpJObject);
            }
            catch { Debug.WriteLine("Error reading PvP stats for " + member.Name); }

            Debug.WriteLine("Processed member " + member.Name);

            return true;
        }
        
        private DateTime? GetResponseLastModified(HttpResponseMessage response)
        {
            if (response.Headers.TryGetValues("last-modified", out IEnumerable<string> values))
            {
                return DateTime.Parse(values.FirstOrDefault());
            }

            return null;
        } 

        private IEnumerable<GuildMember> GetPlayersJsonFromGuild(JObject guildJson)
        {
            var members = guildJson.SelectToken("members").Select(c => new GuildMember()
            {
                Name = c.SelectToken("character.name").ToString(),
                GuildRank = int.Parse(c.SelectToken("rank").ToString()),
                Realm = c.SelectToken("character.realm").ToString(),
                Class = int.Parse(c.SelectToken("character.class").ToString()),
                Level = int.Parse(c.SelectToken("character.level").ToString()),
                AchievementPoints = int.Parse(c.SelectToken("character.achievementPoints").ToString())
            });

            return members;
        }

        private IEnumerable<string> GetPlayerNamesFromGuildJson(JObject json)
        {
            return json.SelectToken("members").Select(m => m.SelectToken("character.name")).Select(n => (string)n);
        }

        private void PopulateItemsDetailsFromJson(GuildMember member, JObject json)
        {
            var items = json.SelectToken("items");

            member.EquippedIlvl = int.Parse(items["averageItemLevelEquipped"].ToString());
            member.MaximumIlvl = int.Parse(items["averageItemLevel"].ToString());

            var neckNode = items.SelectToken("neck");
            
            if (neckNode["name"].ToString() == "Heart of Azeroth")
            {
                member.AzeriteLevel = int.Parse(neckNode.SelectToken("azeriteItem.azeriteLevel").ToString());
            }
        }

        private void PopulatePetCountFromJson(GuildMember member, JObject petsJson)
        {
            var petsNode = petsJson.SelectToken("pets");

            member.PetCount = int.Parse(petsNode["numCollected"].ToString());
        }

        private void PopulateMountCountFromJson(GuildMember member, JObject mountsJson)
        {
            var mountsNode = mountsJson.SelectToken("mounts");

            member.MountCount = int.Parse(mountsNode["numCollected"].ToString());
        }

        private void PopulatePvpStatsFromJson(GuildMember member, JObject pvpJson)
        {
            var pvpNode = pvpJson.SelectToken("pvp");
            var bracketsNode = pvpNode.SelectToken("brackets");

            member.Pvp2v2Rating = int.Parse(bracketsNode.SelectToken("ARENA_BRACKET_2v2.rating").ToString());
            member.Pvp3v3Rating = int.Parse(bracketsNode.SelectToken("ARENA_BRACKET_3v3.rating").ToString());
            member.PvpRbgRating = int.Parse(bracketsNode.SelectToken("ARENA_BRACKET_RBG.rating").ToString());
            member.TotalHonorableKills = int.Parse(pvpJson["totalHonorableKills"].ToString());
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
