using System;
using System.Collections.Generic;

namespace GuildTools.Controllers.Models
{
    public class BlizzardPlayer
    {
        public string PlayerName { get; set; }
        public string PlayerRealmName { get; set; }
        public int Class { get; set; }
        public int Level { get; set; }
        public string GuildName { get; set; }
        public string GuildRealm { get; set; }
    }
}
