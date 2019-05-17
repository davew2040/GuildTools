using GuildTools.Controllers;
using GuildTools.Controllers.JsonResponses;
using GuildTools.Controllers.Models;
using GuildTools.Data.RepositoryModels;
using GuildTools.EF.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EfEnums = GuildTools.EF.Models.Enums;
using EfModels = GuildTools.EF.Models;
using RepoModels = GuildTools.Data.RepositoryModels;
using ControllerModels = GuildTools.Controllers.Models;
using ControllerInputModels = GuildTools.Controllers.InputModels;
using EfBlizzardModels = GuildTools.EF.Models.StoredBlizzardModels;
using GuildTools.EF.Models.Enums;

namespace GuildTools.Data
{
    public interface IDataRepository : IDisposable
    {
        Task<EfEnums.GuildProfilePermissionLevel?> GetProfilePermissionForUserAsync(int profileId, string userId);
        Task<bool> ProfileIsPublicAsync(int profileId);
        Task<int> CreateGuildProfileAsync(
            string creatorId, 
            string profileName, 
            GuildSlim guild, 
            EfModels.StoredBlizzardModels.StoredRealm realm, 
            EfEnums.GameRegionEnum region, 
            bool isPublic);
        Task<IEnumerable<EfBlizzardModels.StoredPlayer>> InsertGuildPlayersIfNeededAsync(
            IEnumerable<EfBlizzardModels.StoredPlayer> players, 
            int profileId, 
            int guildId);
        Task<EfModels.PlayerMain> AddMainToProfileAsync(int playerId, int profileId);
        Task<EfModels.PlayerAlt> AddAltToMainAsync(int playerId, int mainId, int profileId);
        Task RemoveAltFromMainAsync(int mainId, int altId, int profileId);
        Task<EfModels.PlayerMain> PromoteAltToMainAsync(int altId, int profileId);
        Task RemoveMainAsync(int mainId, int profileId);
        Task<IdentityUser> GetUserByEmailAddressAsync(string email);
        Task<IEnumerable<EfModels.GuildProfile>> GetGuildProfilesForUserAsync(string userId);
        Task<EfModels.GuildProfile> GetFullGuildProfileAsync(int id);
        Task<EfModels.GuildProfile> GetGuildProfile_GuildAndFriendGuildsAsync(int id);
        Task DeleteProfileAsync(int id);
        Task EditPlayerNotes(int profileId, int playerMainId, string newNotes);
        Task EditOfficerNotes(int profileId, int playerMainId, string newNotes);

        Task AddAccessRequestAsync(string userId, int profileId);
        Task<IEnumerable<EfModels.PendingAccessRequest>> GetAccessRequestsAsync(int profileId);
        Task ApproveAccessRequest(int requestId);
        Task<IEnumerable<RepoModels.ProfilePermissionByUser>> GetFullProfilePermissions(string userId, int profileId);
        Task UpdatePermissions(string userId, IEnumerable<ControllerInputModels.UpdatePermission> newPermissions, int profileId, bool isAdmin);
        Task<IEnumerable<EfModels.FriendGuild>> GetFriendGuilds(int profileId);
        Task<EfModels.FriendGuild> AddFriendGuild(int profileId, EfModels.StoredBlizzardModels.StoredGuild storedGuild);
        Task DeleteFriendGuildAsync(int profileId, int friendGuildId);

        Task AddNotification(string email, string operationKey, NotificationRequestTypeEnum type);
        Task<IEnumerable<NotificationRequest>> GetAndClearNotifications(NotificationRequestTypeEnum type, string operationKey = null);

        Task<CachedValue> GetCachedValueAsync(string key);
        Task SetCachedValueAsync(string key, string value, TimeSpan duration);
        Task<string> GetStoredValueAsync(string key);
        Task CreateOrUpdateStoredValueAsync(string key, string value);
    }
}
