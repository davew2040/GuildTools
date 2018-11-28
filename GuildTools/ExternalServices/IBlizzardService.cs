using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using static GuildTools.ExternalServices.BlizzardService;

namespace GuildTools.ExternalServices
{
    public interface IBlizzardService
    {
        Task<string> GetGuildMembers(string guild, string realm, Region region);
        Task<string> GetPlayerItems(string player, string realm, Region region);
        Task<string> GetPlayerMounts(string player, string realm, Region region);
        Task<string> GetPlayerPets(string player, string realm, Region region);
        Task<string> GetPlayerPvpStats(string player, string realm, Region region);
    }
}
