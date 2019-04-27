using System;
using System.Collections.Generic;

namespace GuildTools.EF.Models
{
    public partial class UserData
    {
        public string UserId { get; set; }
        public string Username { get; set; }
        public string GuildName { get; set; }
        public string GuildRealm { get; set; }

        public virtual ICollection<GuildProfile> GuildProfiles { get; set; }
    }
}
