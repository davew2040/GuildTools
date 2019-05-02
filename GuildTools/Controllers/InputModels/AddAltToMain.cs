using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuildTools.Controllers.InputModels
{
    public class AddMainToProfile
    {
        public string PlayerName { get; set; }
        public string GuildName { get; set; }
        public string PlayerRealmName { get; set; }
        public string GuildRealmName { get; set; }
        public int ProfileId { get; set; }
        public string RegionName { get; set; }
    }
}
