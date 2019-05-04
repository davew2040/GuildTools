using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EfEnums = GuildTools.EF.Models.Enums;

namespace GuildTools.Data.RepositoryModels
{
    public class GuildToolsUserStub
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
    }
}
