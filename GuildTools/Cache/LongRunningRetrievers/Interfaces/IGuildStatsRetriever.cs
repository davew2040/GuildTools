﻿using GuildTools.Controllers.JsonResponses;
using GuildTools.ExternalServices.Blizzard.Utilities;
using GuildTools.Scheduler;
using GuildTools.Services;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static GuildTools.ExternalServices.Blizzard.BlizzardService;

namespace GuildTools.Cache.LongRunningRetrievers.Interfaces
{
    public interface IGuildStatsRetriever
    {
        Task<CacheEntry<IEnumerable<GuildMemberStats>>> GetCachedEntry(BlizzardRegion region, string realm, string guild);
        string GetKey(BlizzardRegion region, string realm, string guild);
        int? GetPositionInQueue(BlizzardRegion region, string realm, string guild);
    }
}
