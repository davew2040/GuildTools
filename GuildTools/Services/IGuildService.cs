using GuildTools.Controllers.JsonResponses;
using GuildTools.Controllers.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static GuildTools.ExternalServices.Blizzard.BlizzardService;

namespace GuildTools.Services
{
    public interface IGuildService
    {
        Task<IEnumerable<BlizzardPlayer>> GetSlimGuildMembersDataAsync(BlizzardRegion region, string guild, string realm);
        Task<BlizzardPlayer> GetSinglePlayerAsync(BlizzardRegion region, string realmName, string playerName);
        Task<IEnumerable<GuildMemberStats>> GetLargeGuildMembersDataAsync(BlizzardRegion region, string guild, string realm);
        Task<GuildSlim> GetGuild(BlizzardRegion region, string realmName, string playerName);
        Task<Realm> GetRealmAsync(string realmName, BlizzardRegion region);
    }
}
