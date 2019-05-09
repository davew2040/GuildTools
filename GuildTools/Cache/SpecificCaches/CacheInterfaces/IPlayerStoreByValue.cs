using GuildTools.Controllers.JsonResponses;
using GuildTools.EF.Models.Enums;
using GuildTools.EF.Models.StoredBlizzardModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GuildTools.Cache.SpecificCaches.CacheInterfaces
{
    public interface IPlayerStoreByValue
    {
        Task<StoredPlayer> GetPlayerAsync(string playerName, StoredRealm realm, StoredGuild guild, int profileId);
        Task<StoredPlayer> GetPlayersAsync(IEnumerable<StoredPlayer> players);
        Task InsertPlayerAsync(StoredPlayer player, int profileId);
    }
}
