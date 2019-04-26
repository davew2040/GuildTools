using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using static GuildTools.ExternalServices.Blizzard.BlizzardService;

namespace GuildTools.ExternalServices.Blizzard
{
    public interface IBlizzardService
    {
        Task<string> GetGuildMembersAsync(string guild, string realm, BlizzardRegion region);
        Task<string> GetPlayerItemsAsync(string player, string realm, BlizzardRegion region);
        Task<string> GetPlayerMountsAsync(string player, string realm, BlizzardRegion region);
        Task<string> GetPlayerPetsAsync(string player, string realm, BlizzardRegion region);
        Task<string> GetPlayerPvpStatsAsync(string player, string realm, BlizzardRegion region);
        Task<string> GetGuildAsync(string guild, string realm, BlizzardRegion region);
        Task<string> GetRealmsByRegionAsync(BlizzardRegion region);
    }
}
