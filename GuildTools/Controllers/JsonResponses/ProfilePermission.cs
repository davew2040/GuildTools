using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuildTools.Controllers.JsonResponses
{
    public class ProfilePermission
    {
        public int ProfileId { get; set; }
        public int PermissionLevel { get; set; }
    }
}
