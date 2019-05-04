using System;
using System.Collections.Generic;

namespace GuildTools.Controllers.Models
{
    public class FullProfilePermissions
    {
        public IEnumerable<ProfilePermissionByUser> Permissions { get; set; }
        public int CurrentPermissions { get; set; }
    }
}
