using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuildTools.Cache
{
    public static class Keyifier
    {
        public static string GetGuildKey(string guildName)
        {
            guildName = guildName.ToLower();
            guildName.Replace(" ", "-");

            return guildName;
        }
    }
}
