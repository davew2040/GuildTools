using GuildTools.Controllers.JsonResponses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static GuildTools.ExternalServices.Blizzard.BlizzardService;

namespace GuildTools.Cache
{
    public interface IOldGuildMemberCache
    {
        Task<IEnumerable<GuildMemberStats>> GetAsync(BlizzardRegion region, string realm, string guild);
    }
}
