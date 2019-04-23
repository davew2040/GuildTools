using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EfEnums = GuildTools.EF.Models.Enums;

namespace GuildTools.Data.RepositoryModels
{
    public class ProfilePermission
    {
        public int GuildId { get; set; }
        public EfEnums.GuildProfilePermissionLevel PermissionLevel { get; set; }
    }
}
