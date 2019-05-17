using System;
using System.Collections.Generic;

namespace GuildTools.Controllers.Models
{
    public class FullGuildProfile
    {
        public int Id { get; set; }
        public UserStub Creator { get; set; }
        public string ProfileName { get; set; }
        public string GuildName { get; set; }
        public StoredRealm Realm { get; set; }
        public string Region { get; set; }
        public int? CurrentPermissionLevel { get; set; }
        public int AccessRequestCount { get; set; }
        public bool IsPublic { get; set; }

        public IEnumerable<StoredPlayer> Players { get; set; }
        public IEnumerable<PlayerMain> Mains { get; set; }
        public IEnumerable<StoredPlayer> PlayerPool { get; set; }
        public IEnumerable<FriendGuild> FriendGuilds { get; set; }
    }
}
