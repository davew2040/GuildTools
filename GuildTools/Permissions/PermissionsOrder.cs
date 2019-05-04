using GuildTools.EF.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuildTools.Permissions
{
    public class PermissionsOrder
    {
        private static Dictionary<GuildProfilePermissionLevel, int> PermissionOrderMap =
            new Dictionary<GuildProfilePermissionLevel, int>()
                {
                    { GuildProfilePermissionLevel.Visitor, 1 },
                    { GuildProfilePermissionLevel.Member, 2 },
                    { GuildProfilePermissionLevel.Officer, 3 },
                    { GuildProfilePermissionLevel.Admin, 4 }
                };

        public static int GetPermissionOrder(GuildProfilePermissionLevel level)
        {
            return PermissionOrderMap[level];
        }

        public static bool GreaterThanOrEqual(GuildProfilePermissionLevel target, GuildProfilePermissionLevel compareAgainst)
        {
            return PermissionsOrder.PermissionOrderMap[target] >= PermissionsOrder.PermissionOrderMap[compareAgainst];
        }

        public static int Order(GuildProfilePermissionLevel level)
        {
            return PermissionsOrder.PermissionOrderMap[level];
        }
    }
}
