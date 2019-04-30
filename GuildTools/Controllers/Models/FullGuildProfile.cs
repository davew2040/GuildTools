using System;
using System.Collections.Generic;

namespace GuildTools.Controllers.Models
{
    public class FullGuildProfile
    {
        public int Id { get; set; }
        public CreatorStub Creator { get; set; }
        public string ProfileName { get; set; }
        public string GuildName { get; set; }
        public StoredRealm Realm { get; set; }
        public string Region { get; set; }

        public IEnumerable<BlizzardPlayer> Players { get; set; }
        public IEnumerable<PlayerMain> Mains { get; set; }
    }
}
