using GuildTools.EF.Models.StoredBlizzardModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EfEnums = GuildTools.EF.Models.Enums;

using EfModels = GuildTools.EF.Models;

namespace GuildTools.Data.RepositoryModels
{
    public class ProfilePermission
    {
        public int ProfileId { get; set; }
        public EfEnums.GuildProfilePermissionLevel PermissionLevel { get; set; }
    }
}
