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

namespace GuildTools.Data
{
    public class DataRepository : IDataRepository
    {
        GuildToolsContext context;

        public DataRepository(GuildToolsContext context)
        {
            this.context = context;
        }

        public async Task CreateGuildProfileAsync(string creatorId, string guild, string realm, BlizzardService.Region region)
        {
            using (var transaction = await this.context.Database.BeginTransactionAsync())
            {
                var newProfile = new EF.Models.GuildProfile()
                {
                    CreatorId = creatorId,
                    GuildName = guild,
                    Realm = realm,
                    RegionId = (int)BlizzardUtilities.Utilities.GetEfRegionFromBlizzardRegion(region)
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

        public Task<IEnumerable<ProfilePermission>> GetProfilePermissionsForUserAsync(string userId)
        {
            throw new NotImplementedException();
        }
    }
}
