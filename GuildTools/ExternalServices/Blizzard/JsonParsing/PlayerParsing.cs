using GuildTools.Controllers.JsonResponses;
using GuildTools.ExternalServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace GuildTools.ExternalServices.Blizzard.JsonParsing
{
    public class PlayerParsing
    {
        public static SinglePlayer GetSinglePlayerFromJson(string json)
        {
            var jObject = JsonConvert.DeserializeObject(json) as JObject;

            var name = jObject["name"].ToString();
            var realm = jObject["realm"].ToString();
            var playerClass = int.Parse(jObject["class"].ToString());
            var level = int.Parse(jObject["level"].ToString());

            string guildName = string.Empty;
            string guildRealm = string.Empty;

            var guild = jObject.SelectToken("guild");
            if (guild != null)
            {
                guildName = guild["name"].ToString();
                guildRealm = guild["realm"].ToString();
            }

            return new SinglePlayer()
            {
                Name = name,
                Realm = realm,
                Class = playerClass,
                Level = level,
                GuildName = guildName,
                GuildRealm = guildRealm
            };
        }

        /* Parsed objects */

        public class SinglePlayer
        {
            public string Name { get; set; }
            public string Realm { get; set; }
            public int Class { get; set; }
            public int Level { get; set; }
            public string GuildName { get; set; }
            public string GuildRealm { get; set; }
        }
    }
}
