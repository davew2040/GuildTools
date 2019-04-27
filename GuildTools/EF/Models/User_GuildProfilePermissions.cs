using System;
using System.Collections.Generic;

namespace GuildTools.EF.Models
{
    public partial class User_GuildProfilePermissions
    {
        public string UserId { get; set; }
        public int PermissionLevelId { get; set; }
        public int ProfileId { get; set; }

        public virtual GuildProfile Profile { get; set; }
        public virtual GuildProfilePermissionLevel PermissionLevel { get; set; }
    }
}
