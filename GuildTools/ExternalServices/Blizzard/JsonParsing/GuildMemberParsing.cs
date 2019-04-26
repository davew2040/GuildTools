﻿using GuildTools.Controllers.JsonResponses;
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
    public class GuildMemberParsing
    {
        public static IEnumerable<GuildMember> GetSlimPlayersFromGuildPlayerList(string guildJson)
        {
            var jObject = JsonConvert.DeserializeObject(guildJson) as JObject;

            var members = jObject.SelectToken("members").Select(c => new GuildMember()
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

        public static PlayerItemDetails GetItemsDetailsFromJson(string json)
        {
            PlayerItemDetails itemDetails = new PlayerItemDetails();

            var jObject = JsonConvert.DeserializeObject(json) as JObject;

            var items = jObject.SelectToken("items");

            var lastModified = jObject.SelectToken("lastModified").ToString();

            itemDetails.LastModifiedDateTime = BlizzardService.FromUnixTime(long.Parse(lastModified));
            itemDetails.EquippedIlvl = int.Parse(items["averageItemLevelEquipped"].ToString());
            itemDetails.MaximumIlvl = int.Parse(items["averageItemLevel"].ToString());

            var neckNode = items.SelectToken("neck");

            if (neckNode["name"].ToString() == "Heart of Azeroth")
            {
                itemDetails.AzeriteLevel = int.Parse(neckNode.SelectToken("azeriteItem.azeriteLevel").ToString());
            }

            return itemDetails;
        }

        public static MountDetails GetMountDetailsFromJson(string json)
        {
            var jObject = JsonConvert.DeserializeObject(json) as JObject;

            var petsNode = jObject.SelectToken("mounts");

            return new MountDetails()
            {
                NumberCollected = int.Parse(petsNode["numCollected"].ToString())
            };
        }

        public static PetDetails GetPetDetailsFromJson(string json)
        {
            var jObject = JsonConvert.DeserializeObject(json) as JObject;

            var petsNode = jObject.SelectToken("pets");

            return new PetDetails()
            {
                NumberCollected = int.Parse(petsNode["numCollected"].ToString())
            };
        }

        public static PvpStats GetPvpStatsFromJson(string json)
        {
            PvpStats stats = new PvpStats();

            var jObject = JsonConvert.DeserializeObject(json) as JObject;

            var pvpNode = jObject.SelectToken("pvp");
            var bracketsNode = pvpNode.SelectToken("brackets");

            stats.Pvp2v2Rating = int.Parse(bracketsNode.SelectToken("ARENA_BRACKET_2v2.rating").ToString());
            stats.Pvp3v3Rating = int.Parse(bracketsNode.SelectToken("ARENA_BRACKET_3v3.rating").ToString());
            stats.PvpRbgRating = int.Parse(bracketsNode.SelectToken("ARENA_BRACKET_RBG.rating").ToString());
            stats.TotalHonorableKills = int.Parse(jObject["totalHonorableKills"].ToString());

            return stats;
        }

        /* Parsed objects */

        public class PlayerItemDetails
        {
            public int EquippedIlvl { get; set; }
            public int MaximumIlvl { get; set; }
            public int? AzeriteLevel { get; set; }
            public DateTime LastModifiedDateTime { get; set; }
        }

        public class MountDetails
        {
            public int NumberCollected { get; set; }
        }

        public class PetDetails
        {
            public int NumberCollected { get; set; }
        }

        public class PvpStats
        {
            public int Pvp2v2Rating { get; set; }
            public int Pvp3v3Rating { get; set; }
            public int PvpRbgRating { get; set; }
            public int TotalHonorableKills { get; set; }
        }
    }
}