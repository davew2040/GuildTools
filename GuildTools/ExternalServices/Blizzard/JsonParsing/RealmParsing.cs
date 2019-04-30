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
    public class RealmParsing
    {
        public static IEnumerable<Realm> GetRealms(string realmsJson)
        {
            var jObject = JsonConvert.DeserializeObject(realmsJson) as JObject;

            var realms = jObject.SelectToken("realms").Select(token => new Realm()
            {
                Id = int.Parse(token.SelectToken("id").ToString()),
                Name = token.SelectToken("name").ToString(),
                Slug = token.SelectToken("slug").ToString()
            });

            return realms;
        }

        public static Realm GetRSingleRealm(string realmJson)
        {
            var jObject = JsonConvert.DeserializeObject(realmJson) as JObject;

            var realm = new Realm()
            {
                Id = int.Parse(jObject.SelectToken("id").ToString()),
                Name = jObject.SelectToken("name").ToString(),
                Slug = jObject.SelectToken("slug").ToString()
            };

            return realm;
        }
    }
}
