using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuildTools.Controllers.JsonResponses
{
    public class RaiderIoStats
    {
        public string Name { get; set; }
        public string RealmName { get; set; }
        public string RegionName { get; set; }
        public string GuildName { get; set; }
        public int Class { get; set; }
        public int RaiderIoOverall { get; set; }
        public int RaiderIoDps { get; set; }
        public int RaiderIoTank { get; set; }
        public int RaiderIoHealer { get; set; }
    }
}
