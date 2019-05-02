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
using GuildTools.ErrorHandling;
using System.Net;

namespace GuildTools.Data
{
    public class DataRepository : IDataRepository
    {
        private GuildToolsContext context;

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

        public async Task DeleteProfile(int id)
        {
            var profileTask = this.context.GuildProfile.FirstOrDefaultAsync(p => p.Id == id);
            var playersTask = this.context.StoredPlayers.Where(x => x.ProfileId == id).ToListAsync();
            var guildsTask = this.context.StoredGuilds.Where(x => x.ProfileId == id).ToListAsync();
            var mainsTask = this.context.PlayerMains.Where(x => x.ProfileId == id).ToListAsync();
            var altsTask = this.context.PlayerAlts.Where(x => x.ProfileId == id).ToListAsync();
            var permissionsTask = this.context.User_GuildProfilePermissions.Where(x => x.ProfileId == id).ToListAsync();

            Task.WaitAll(profileTask, playersTask, guildsTask, mainsTask, altsTask, permissionsTask);

            using (var transaction = await this.context.Database.BeginTransactionAsync())
            {
                this.context.User_GuildProfilePermissions.RemoveRange(permissionsTask.Result);
                profileTask.Result.CreatorGuildId = null;

                await this.context.SaveChangesAsync();

                this.context.PlayerAlts.RemoveRange(altsTask.Result);

                await this.context.SaveChangesAsync();

                this.context.PlayerMains.RemoveRange(mainsTask.Result);

                await this.context.SaveChangesAsync();

                this.context.StoredPlayers.RemoveRange(playersTask.Result);

                await this.context.SaveChangesAsync();

                this.context.StoredGuilds.RemoveRange(guildsTask.Result);

                await this.context.SaveChangesAsync();

                this.context.GuildProfile.Remove(profileTask.Result);

                await this.context.SaveChangesAsync();

                transaction.Commit();
            }
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

            await context.Entry(newPlayer).Reference(x => x.Player).LoadAsync();

            return newPlayer;
        }

        public async Task<EfModels.PlayerAlt> AddAltToMainAsync(int playerId, int mainId, int profileId)
        {
            var profileTask = this.context.GuildProfile
                .Include(x => x.PlayerMains)
                .SingleOrDefaultAsync(x => x.Id == profileId);

            var mainTask = this.context.PlayerMains
                .SingleOrDefaultAsync(x => x.Id == mainId);

            var playerTask = this.context.PlayerMains
                .SingleOrDefaultAsync(x => x.Id == playerId);

            Task.WaitAll(profileTask, mainTask, playerTask);

            var newAlt = new EfModels.PlayerAlt()
            {
                ProfileId = profileId,
                PlayerId = playerId,
                PlayerMainId = mainId
            };

            this.context.PlayerAlts.Add(newAlt);

            await this.context.SaveChangesAsync();

            await context.Entry(newAlt).Reference(x => x.Player).LoadAsync();

            return newAlt;
        }

        public async Task RemoveAltFromMainAsync(int mainId, int altId, int profileId)
        {
            var mainTask = this.context.PlayerMains.Include(p => p.Alts)
                .SingleOrDefaultAsync(x => x.Id == mainId);

            var altTask = this.context.PlayerAlts.Include(p => p.PlayerMain)
                .SingleOrDefaultAsync(x => x.Id == altId);

            Task.WaitAll(mainTask, mainTask);

            if (mainTask.Result == null)
            {
                throw new UserReportableError($"No main player found with ID of '{mainId}'.", (int)HttpStatusCode.BadRequest);
            }

            if (altTask.Result == null)
            {
                throw new UserReportableError($"No alt player found with ID of '{altId}'.", (int)HttpStatusCode.BadRequest);
            }

            if (!mainTask.Result.Alts.Any(x => x.Id == altId))
            {
                throw new UserReportableError($"Main player '{mainId}' does not own alt '{altId}'.", (int)HttpStatusCode.BadRequest);
            }

            mainTask.Result.Alts.Remove(altTask.Result);
            this.context.PlayerAlts.Remove(altTask.Result);

            await this.context.SaveChangesAsync();
        }

        public async Task RemoveMainAsync(int mainId, int profileId)
        {
            var mainTask = this.context.PlayerMains.Include(p => p.Alts)
                .SingleOrDefaultAsync(x => x.Id == mainId);

            var altsTask = this.context.PlayerAlts
                .Where(x => x.PlayerMainId == mainId)
                .ToListAsync();

            Task.WaitAll(mainTask, altsTask);

            if (mainTask.Result == null)
            {
                throw new UserReportableError($"No main player found with ID of '{mainId}'.", (int)HttpStatusCode.BadRequest);
            }

            this.context.PlayerAlts.RemoveRange(altsTask.Result);
            this.context.PlayerMains.Remove(mainTask.Result);

            await this.context.SaveChangesAsync();
        }

        public async Task<string> GetStoredValueAsync(string key)
        {
            var item = await this.context.ValueStore.SingleOrDefaultAsync(x => x.Id == key);

            return item?.Value;
        }

        public async Task CreateOrUpdateStoredValueAsync(string key, string value)
        {
            var item = await this.context.ValueStore.SingleOrDefaultAsync(x => x.Id == key);

            if (item == null)
            {
                this.context.ValueStore.Add(new ValueStore()
                {
                    Id = key,
                    Value = value
                });
                await this.context.SaveChangesAsync();
                return;
            }

            item.Value = value;
            await this.context.SaveChangesAsync();
            return;
        }
    }
}
