using GuildTools.Controllers.JsonResponses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuildTools.Controllers.Models
{
    public class AggregatedProfileGuildStatsResponse
    {
        public bool IsCompleted { get; set; }
        public IEnumerable<IndividualAggregatedStatsItem> IndividualGuildResponses { get; set; }
        public IEnumerable<GuildMemberStats> Values { get; set; }
    }

    public class IndividualAggregatedStatsItem
    {
        public GuildStatsResponse IndividualStats { get; set; }
        public string GuildName { get; set; }
        public string RealmName { get; set; }
        public string RegionName { get; set; }
    }
}
