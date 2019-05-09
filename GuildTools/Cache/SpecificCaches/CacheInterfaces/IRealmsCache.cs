using GuildTools.Controllers.JsonResponses;
using GuildTools.Controllers.Models;
using GuildTools.ExternalServices;
using GuildTools.ExternalServices.Blizzard;
using GuildTools.ExternalServices.Blizzard.JsonParsing;
using GuildTools.ExternalServices.Blizzard.Utilities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static GuildTools.ExternalServices.Blizzard.BlizzardService;
using EfEnums = GuildTools.EF.Models.Enums;

namespace GuildTools.Cache.SpecificCaches.CacheInterfaces
{
    public interface IRealmsCache
    {
        Task<IEnumerable<Realm>> GetRealms(EfEnums.GameRegionEnum region);
    }
}
