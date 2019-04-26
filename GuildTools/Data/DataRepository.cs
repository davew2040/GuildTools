using GuildTools.Data.RepositoryModels;
using GuildTools.EF;
using GuildTools.ExternalServices.Blizzard;
using BlizzardUtilities = GuildTools.ExternalServices.Blizzard.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GuildTools.EF.Models;
using EfEnums = GuildTools.EF.Models.Enums;
using Microsoft.AspNetCore.Identity;

namespace GuildTools.Data
{
    public class DataRepository : IDataRepository
    {
        GuildToolsContext context;

        public DataRepository(
            GuildToolsContext context, 
            IBlizzardService blizzardService)
        {
            this.context = context;
        }

        public async Task CreateGuildProfileAsync(string creatorId, string guild, string realm, BlizzardService.BlizzardRegion region)
        {
            using (var transaction = await this.context.Database.BeginTransactionAsync())
            {
                var newProfile = new EF.Models.GuildProfile()
                {
                    CreatorId = creatorId,
                    GuildName = guild,
                    Realm = realm,
                    RegionId = (int)BlizzardUtilities.BlizzardUtilities.GetEfRegionFromBlizzardRegion(region)
                };

                this.context.GuildProfile.Add(newProfile);

                await this.context.SaveChangesAsync();

                this.context.User_GuildProfilePermissions.Add(new EF.Models.User_GuildProfilePermissions()
                {
                    PermissionLevelId = (int)EF.Models.Enums.GuildProfilePermissionLevel.Admin,
                    ProfileId = newProfile.Id,
                    UserId = creatorId
                });

                await this.context.SaveChangesAsync();
            }
        }

        public async Task<CachedValue> GetCachedValueAsync(string key)
        {
            var result = await this.context.BigValueCache.FindAsync(key);

            if (result == null)
            {
                return null;
            }

            if (result.ExpiresOn < DateTime.Now)
            {
                this.context.Remove(result);
                await this.context.SaveChangesAsync();

                return null;
            }

            return new CachedValue()
            {
                Key = result.Id,
                Value = result.Value,
                ExpiresOn = result.ExpiresOn
            };
        }

        public async Task<IEnumerable<GuildProfile>> GetGuildProfilesForUserAsync(string userId)
        {
            return await this.context.GuildProfile
                .Where(p => p.CreatorId == userId)
                .Include(x => x.Region)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProfilePermission>> GetProfilePermissionsForUserAsync(string userId)
        {
            var query = await this.context.User_GuildProfilePermissions
                .Where(p => p.UserId == userId)
                .ToListAsync();

            return query
                .Select(p => new ProfilePermission()
                {
                    ProfileId = p.ProfileId,
                    PermissionLevel = Enum.Parse<EfEnums.GuildProfilePermissionLevel>(p.PermissionLevelId.ToString())
                });
        }

        public async Task<IdentityUser> GetUserByEmailAddressAsync(string email)
        {
            return await this.context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }
    }
}
