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

namespace GuildTools.Data
{
    public interface IDataRepository : IDisposable
    {
        Task<EfEnums.GuildProfilePermissionLevel?> GetProfilePermissionForUserAsync(int profileId, string userId);
        Task CreateGuildProfileAsync(string creatorId, string profileName, GuildSlim guild, EfModels.StoredBlizzardModels.StoredRealm realm, EfEnums.GameRegion region);
        Task<EfModels.PlayerMain> AddMainToProfileAsync(int playerId, int profileId);
        Task<EfModels.PlayerAlt> AddAltToMainAsync(int playerId, int mainId, int profileId);
        Task RemoveAltFromMainAsync(int mainId, int altId, int profileId);
        Task RemoveMainAsync(int mainId, int profileId);
        Task<IdentityUser> GetUserByEmailAddressAsync(string email);
        Task<IEnumerable<EfModels.GuildProfile>> GetGuildProfilesForUserAsync(string userId);
        Task<RepoModels.FullGuildProfile> GetFullGuildProfileAsync(int id);
        Task DeleteProfileAsync(int id);

        Task AddAccessRequestAsync(string userId, int profileId);
        Task<IEnumerable<EfModels.PendingAccessRequest>> GetAccessRequestsAsync(int profileId);
        Task ApproveAccessRequest(int requestId);
        Task<IEnumerable<RepoModels.ProfilePermissionByUser>> GetFullProfilePermissions(string userId, int profileId);
        Task UpdatePermissions(string userId, IEnumerable<ControllerInputModels.UpdatePermission> newPermissions, int profileId, bool isAdmin);

        Task<CachedValue> GetCachedValueAsync(string key);
        Task SetCachedValueAsync(string key, string value, TimeSpan duration);
        Task<string> GetStoredValueAsync(string key);
        Task CreateOrUpdateStoredValueAsync(string key, string value);
    }
}
