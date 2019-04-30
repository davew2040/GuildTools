using GuildTools.Controllers.JsonResponses;
using GuildTools.Controllers.Models;
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
    public class GuildParsing
    {
        public static GuildSlim GetGuild(string guildJson)
        {
            var jObject = JsonConvert.DeserializeObject(guildJson) as JObject;

            var guild = new GuildSlim()
            {
                Name = jObject.SelectToken("name").ToString(),
                Realm = jObject.SelectToken("realm").ToString(),
                Battlegroup = jObject.SelectToken("battlegroup").ToString()
            };

            return guild;
        }
    }
}
