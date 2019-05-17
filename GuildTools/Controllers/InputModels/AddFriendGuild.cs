using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuildTools.Controllers.InputModels
{
    public class AddFriendGuild
    {
        public int ProfileId { get; set; }
        public string RegionName { get; set; }
        public string RealmName { get; set; }
        public string GuildName { get; set; }
    }
}
