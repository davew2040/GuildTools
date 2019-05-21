using GuildTools.Controllers.JsonResponses;
using GuildTools.ExternalServices;
using GuildTools.ExternalServices.Blizzard.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace GuildTools.ExternalServices.Raiderio.JsonParsing
{
    public class RaiderIoParsing
    {
        public static bool GetRequestSucceeded(string json)
        {
            var jObject = JsonConvert.DeserializeObject(json) as JObject;

            return jObject["statusCode"] == null;
        }

        public static RaiderIoStats GetPlayerFromJson(string playerJson)
        {
            var jObject = JsonConvert.DeserializeObject(playerJson) as JObject;

            var newPlayer = new RaiderIoStats();

            newPlayer.Name = jObject["name"].ToString();
            newPlayer.RealmName = jObject["realm"].ToString();
            newPlayer.Class = BlizzardUtilities.GetBlizzardClassFromString(jObject["class"].ToString());

            var scoresToken = jObject.SelectToken("mythic_plus_scores");

            newPlayer.RaiderIoOverall = (int)double.Parse(scoresToken["all"].ToString());
            newPlayer.RaiderIoDps = (int)double.Parse(scoresToken["dps"].ToString());
            newPlayer.RaiderIoHealer = (int)double.Parse(scoresToken["healer"].ToString());
            newPlayer.RaiderIoTank = (int)double.Parse(scoresToken["tank"].ToString());

            return newPlayer;
        }
    }
}
