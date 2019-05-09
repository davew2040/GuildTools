using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuildTools.Controllers.InputModels
{
    public class CreateNewGuildProfile
    {
        public string ProfileName { get; set; }
        public string GuildName { get; set; }
        public string GuildRealmName { get; set; }
        public string RegionName { get; set; }
        public bool IsPublic { get; set; }
    }
}
