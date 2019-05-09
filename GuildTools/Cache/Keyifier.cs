using GuildTools.EF.Models.Enums;
using GuildTools.ExternalServices.Blizzard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuildTools.Cache
{
    public static class Keyifier
    {
        public static string GetPlayerKey(string player)
        {
            return player.ToLower();
        }

        public static string GetGuildKey(string guildName)
        {
            guildName = guildName.ToLower();
            guildName.Replace(" ", "-");

            return guildName;
        }

        public static string GetRealmKey(string realmName)
        {
            return BlizzardService.BuildRealmSlug(realmName);
        }

        public static string GetRegionKey(GameRegionEnum region)
        {
            return region.ToString().ToLower();
        }

        public static string GetKey(
            string prefix, IEnumerable<string> keyValues)
        {
            var joinedValues = string.Join(':', keyValues);
            return $"{prefix}_{joinedValues}";
        }
    }
}
