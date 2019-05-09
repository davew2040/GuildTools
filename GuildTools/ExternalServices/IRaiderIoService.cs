using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using static GuildTools.ExternalServices.Blizzard.BlizzardService;

namespace GuildTools.ExternalServices
{
    public interface IRaiderIoService
    {
        Task<string> GetMythicPlusDungeonData(BlizzardRegion region, string playerName, string realm);
    }
}
