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

namespace GuildTools.Data
{
    public interface IDataRepository
    {
        Task<EfEnums.GuildProfilePermissionLevel?> GetProfilePermissionForUserAsync(int profileId, string userId);
        Task CreateGuildProfileAsync(string creatorId, string profileName, GuildSlim guild, EfModels.StoredBlizzardModels.StoredRealm realm, EfEnums.GameRegion region);
        Task<EfModels.PlayerMain> AddMainToProfileAsync(int playerId, int profileId);
        Task<IdentityUser> GetUserByEmailAddressAsync(string email);
        Task<IEnumerable<EfModels.GuildProfile>> GetGuildProfilesForUserAsync(string userId);
        Task<CachedValue> GetCachedValueAsync(string key);
        Task SetCachedValueAsync(string key, string value, TimeSpan duration);
        Task<RepoModels.FullGuildProfile> GetFullGuildProfile(int id);
    }
}
