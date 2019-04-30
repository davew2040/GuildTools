using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static GuildTools.ExternalServices.Blizzard.BlizzardService;

namespace GuildTools.Cache
{
    public interface IGuildStatsCache
    {
        Task<string> Get(BlizzardRegion region, string realm, string guild);
    }
}
