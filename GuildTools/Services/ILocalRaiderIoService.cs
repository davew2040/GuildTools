using GuildTools.Controllers.JsonResponses;
using GuildTools.Controllers.Models;
using GuildTools.ExternalServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static GuildTools.ExternalServices.Blizzard.BlizzardService;

namespace GuildTools.Services
{
    public interface ILocalRaiderIoService
    {
        Task<IEnumerable<RaiderIoStats>> GetGuildRaiderIoStats(BlizzardRegion region, string guild, string realm, IProgress<double> progress);
    }
}
