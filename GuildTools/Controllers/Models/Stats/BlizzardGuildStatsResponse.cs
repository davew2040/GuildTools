﻿using GuildTools.Controllers.JsonResponses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuildTools.Controllers.Models.Stats
{
    public class BlizzardGuildStatsResponse
    {
        public bool IsCompleted { get; set; }
        public int? PositionInQueue { get; set; }
        public double CompletionProgress { get; set; }
        public IEnumerable<GuildMemberStats> Values { get; set; }
    }
}
