using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace GuildTools.ExternalServices
{
    public interface IBlizzardService
    {
        Task<string> GetGuildMembers(string guild, string realm);
        Task<string> GetPlayerItems(string realm, string player);
        Task<HttpResponseMessage> GetPlayerItemsWithResponse(string realm, string player);
        Task<string> GetPlayerMounts(string realm, string player);
        Task<string> GetPlayerPets(string realm, string player);
        Task<string> GetPlayerPvpStats(string realm, string player);
    }
}
