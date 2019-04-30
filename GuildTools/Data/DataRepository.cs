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
using GuildTools.Controllers.Models;
using GuildTools.EF.Models.StoredBlizzardModels;
using EfModels = GuildTools.EF.Models;
using ControllerModels = GuildTools.Controllers.Models;

namespace GuildTools.Data
{
    public class DataRepository : IDataRepository
    {
        GuildToolsContext context;

        public DataRepository(
            GuildToolsContext context)
        {
            this.context = context;
        }

        public async Task CreateGuildProfileAsync(
            string creatorId, 
            string profileName, 
            GuildSlim guild,
            EfModels.StoredBlizzardModels.StoredRealm realm, 
            EfEnums.GameRegion region)
        {
            using (var transaction = await this.context.Database.BeginTransactionAsync())
            {
                var newProfile = new EF.Models.GuildProfile()
                {
                    CreatorId = creatorId,
                    ProfileName = profileName,
                    RealmId = realm.Id
                };

                this.context.GuildProfile.Add(newProfile);

                await this.context.SaveChangesAsync();

                EfModels.StoredBlizzardModels.StoredGuild storedGuild = new EfModels.StoredBlizzardModels.StoredGuild()
                {
                    Name = guild.Name,
                    RealmId = realm.Id,
                    ProfileId = newProfile.Id
                };

                this.context.StoredGuilds.Add(storedGuild);
                newProfile.CreatorGuild = storedGuild;

                this.context.User_GuildProfilePermissions.Add(new EF.Models.User_GuildProfilePermissions()
                {
                    PermissionLevelId = (int)EF.Models.Enums.GuildProfilePermissionLevel.Admin,
                    ProfileId = newProfile.Id,
                    UserId = creatorId
                });

                await this.context.SaveChangesAsync();

                transaction.Commit();
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

        public async Task SetCachedValueAsync(string key, string value, TimeSpan duration)
        {
            using (var transaction = await this.context.Database.BeginTransactionAsync())
            {
                var result = await this.context.BigValueCache.FindAsync(key);

                if (result != null)
                {
                    this.context.BigValueCache.Remove(result);
                }

                if (result.ExpiresOn < DateTime.Now)
                {
                    this.context.BigValueCache.Remove(result);
                }

                await this.context.SaveChangesAsync();

                this.context.BigValueCache.Add(new BigValueCache()
                {
                    Value = value,
                    ExpiresOn = DateTime.Now + duration
                });

                await this.context.SaveChangesAsync();

                transaction.Commit();
            }
        }

        public async Task<IEnumerable<EfModels.GuildProfile>> GetGuildProfilesForUserAsync(string userId)
        {
            return await this.context.GuildProfile
                .Include(g => g.Realm)
                .ThenInclude(g => g.Region)
                .Include(g => g.CreatorGuild)
                .Include(g => g.Creator)
                .Where(g => g.CreatorId == userId)
                .ToListAsync();
        }

        public async Task<EfEnums.GuildProfilePermissionLevel?> GetProfilePermissionForUserAsync(int profileId, string userId)
        {
            var query = await this.context.User_GuildProfilePermissions
                .Include(p => p.PermissionLevel)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.UserId == userId && p.ProfileId == profileId);

            if (query == null)
            {
                return null;
            }

            return (EfEnums.GuildProfilePermissionLevel)query.PermissionLevel.Id;
        }

        public async Task<IdentityUser> GetUserByEmailAddressAsync(string email)
        {
            return await this.context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<RepositoryModels.FullGuildProfile> GetFullGuildProfile(int id)
        {
            var profile = await this.context.GuildProfile
                .Include(x => x.Creator)
                .Include(x => x.CreatorGuild)
                .Include(x => x.PlayerMains)
                    .ThenInclude(y => y.Alts)
                        .ThenInclude(z => z.Player)
                .Include(x => x.PlayerMains)
                    .ThenInclude(y => y.Player)
                .Include(x => x.Realm)
                    .ThenInclude(y => y.Region)
                .Include(x => x.User_GuildProfilePermissions)
                    .ThenInclude(y => y.PermissionLevel)
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == id);

            return new RepositoryModels.FullGuildProfile()
            {
                Profile = profile
            };
        }

        public async Task<EfModels.PlayerMain> AddMainToProfileAsync(int playerId, int profileId)
        {
            var profile = await this.context.GuildProfile
                .Include(x => x.PlayerMains)
                .SingleOrDefaultAsync(x => x.Id == profileId);

            var newPlayer = new EfModels.PlayerMain()
            {
                ProfileId = profileId,
                PlayerId = playerId
            };

            profile.PlayerMains.Add(newPlayer);

            await this.context.SaveChangesAsync();

            return newPlayer;
        }
    }
}
