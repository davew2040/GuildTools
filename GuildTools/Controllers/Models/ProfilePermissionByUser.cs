using System;
using System.Collections.Generic;

namespace GuildTools.Controllers.Models
{
    public class ProfilePermissionByUser
    {
        public UserStub User { get; set; }
        public int PermissionLevel { get; set; }
    }
}
