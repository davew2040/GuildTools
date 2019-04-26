using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuildTools.Controllers.JsonResponses
{
    public class GuildExists
    {
        public bool Found { get; set; }
        public string RealmName { get; set; }
        public string GuildName { get; set; }
    }
}
