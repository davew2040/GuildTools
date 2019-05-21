using GuildTools.Controllers.JsonResponses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuildTools.Controllers.Models.Stats
{
    public class AggregatedProfileRaiderIoStatsResponse
    {
        public bool IsCompleted { get; set; }
        public IEnumerable<AggregatedProfileRaiderIoStatsItem> IndividualGuildResponses { get; set; }
        public IEnumerable<RaiderIoStats> Values { get; set; }
        public FullGuildProfile Profile { get; set; }
    }

    public class AggregatedProfileRaiderIoStatsItem
    {
        public RaiderIoStatsResponse IndividualStats { get; set; }
        public string GuildName { get; set; }
        public string RealmName { get; set; }
        public string RegionName { get; set; }
    }
}
