using System;
using System.Collections.Generic;

namespace GuildTools.Controllers.Models
{
    public class GuildProfileSlim
    {
        public int Id { get; set; }
        public UserStub Creator { get; set; }
        public string ProfileName { get; set; }
        public string RealmName { get; set; }
        public string RegionName { get; set; }
        public string PrimaryGuildName { get; set; }
        public bool IsPublic { get; set; }
    }
}
