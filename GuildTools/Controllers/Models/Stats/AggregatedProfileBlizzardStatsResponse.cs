using GuildTools.Controllers.JsonResponses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuildTools.Controllers.Models.Stats
{
    public class AggregatedProfileBlizzardStatsResponse
    {
        public bool IsCompleted { get; set; }
        public IEnumerable<AggregatedProfileBlizzardStatsItem> IndividualGuildResponses { get; set; }
        public IEnumerable<GuildMemberStats> Values { get; set; }
        public FullGuildProfile Profile { get; set; }
    }

    public class AggregatedProfileBlizzardStatsItem
    {
        public BlizzardGuildStatsResponse IndividualStats { get; set; }
        public string GuildName { get; set; }
        public string RealmName { get; set; }
        public string RegionName { get; set; }
    }
}
