using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuildTools.Permissions
{
    public class GuildToolsRoles
    {
        public static class AdminRole
        {
            public const string Name = "Admin";
        }

        public static class StandardUser
        {
            public const string Name = "Standard";
        }

        public static IEnumerable<string> AllRoleNames
        {
            get
            {
                return new List<string>()
                {
                    AdminRole.Name,
                    StandardUser.Name
                };
            }
        }
    }
}
