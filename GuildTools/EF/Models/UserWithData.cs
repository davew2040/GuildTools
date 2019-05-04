using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GuildTools.EF.Models
{
    public partial class UserWithData : IdentityUser
    {
        public string PlayerName { get; set; }
        public string PlayerRealm { get; set; }
        public string GuildName { get; set; }
        public string GuildRealm { get; set; }
        public string PlayerRegion { get; set; }

        public virtual ICollection<GuildProfile> GuildProfiles { get; set; }
        public virtual ICollection<PendingAccessRequest> PendingAccessRequests { get; set; }
    }
}
