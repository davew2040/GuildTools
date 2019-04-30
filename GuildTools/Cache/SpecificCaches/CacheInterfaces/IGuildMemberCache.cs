using GuildTools.Controllers.JsonResponses;
using GuildTools.Controllers.Models;
using GuildTools.EF.Models.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GuildTools.Cache.SpecificCaches.CacheInterfaces
{
    public interface IGuildMemberCache
    {
        Task<IEnumerable<BlizzardPlayer>> GetMembers(GameRegion region, string realmName, string guildName);
    }
}
