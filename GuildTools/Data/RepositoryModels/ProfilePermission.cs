using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EfEnums = GuildTools.EF.Models.Enums;

using EfModels = GuildTools.EF.Models;

namespace GuildTools.Data.RepositoryModels
{
    public class FullGuildProfile
    {
        public string Name { get; set; }
        public EfModels.GuildProfile Profile { get; set; }
    }
}
