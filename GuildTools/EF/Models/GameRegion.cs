using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuildTools.EF.Models
{
    public class GameRegion
    {
        public int Id { get; set; }
        public string RegionName { get; set; }

        public virtual IEnumerable<GuildProfile> GuildProfiles { get; set; }
    }
}
