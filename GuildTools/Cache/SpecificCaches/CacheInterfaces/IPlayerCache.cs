using GuildTools.Controllers.JsonResponses;
using GuildTools.Controllers.Models;
using GuildTools.EF.Models.Enums;
using System.Threading.Tasks;

namespace GuildTools.Cache.SpecificCaches.CacheInterfaces
{
    public interface IPlayerCache
    {
        Task<BlizzardPlayer> GetPlayer(GameRegionEnum region, string playerName, string realmName);
    }
}
