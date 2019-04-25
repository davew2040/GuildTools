using GuildTools.Data.RepositoryModels;
using GuildTools.EF.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static GuildTools.ExternalServices.Blizzard.BlizzardService;

namespace GuildTools.Data
{
    public interface IDataRepository
    {
        Task<IEnumerable<ProfilePermission>> GetProfilePermissionsForUserAsync(string userId);
        Task CreateGuildProfileAsync(string creatorId, string guild, string realm, Region region);
        Task<IdentityUser> GetUserByEmailAddressAsync(string email);
        Task<IEnumerable<GuildProfile>> GetGuildProfilesForUserAsync(string userId);
    }
}
