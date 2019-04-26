using GuildTools.Controllers.JsonResponses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static GuildTools.ExternalServices.Blizzard.BlizzardService;

namespace GuildTools.Services
{
    public interface IGuildMemberService
    {
        Task<IEnumerable<GuildMember>> GetGuildMemberDataAsync(BlizzardRegion region, string guild, string realm);
    }
}
