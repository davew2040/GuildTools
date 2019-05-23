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
using EfBlizzardModels = GuildTools.EF.Models.StoredBlizzardModels;
using ControllerModels = GuildTools.Controllers.Models;
using GuildTools.ErrorHandling;
using System.Net;
using GuildTools.Controllers.InputModels;
using GuildTools.Permissions;
using GuildTools.Cache.SpecificCaches.CacheInterfaces;
using GuildTools.Cache.SpecificCaches;

namespace GuildTools.Data
{
    public class DataRepository : IDataRepository
    {
        private readonly GuildToolsContext context;

        public DataRepository(
            GuildToolsContext context)
        {
            this.context = context;
        }

        public async Task<int> CreateGuildProfileAsync(
            string creatorId, 
            string profileName, 
            GuildSlim guild,
            EfModels.StoredBlizzardModels.StoredRealm realm, 
            EfEnums.GameRegionEnum region, 
            bool isPublic)
        {
            using (var transaction = await this.context.Database.BeginTransactionAsync())
            {
                var newProfile = new EF.Models.GuildProfile()
                {
                    CreatorId = creatorId,
                    ProfileName = profileName,
                    RealmId = realm.Id,
                    IsPublic = isPublic
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

                if (!isPublic)
                {
                    this.context.User_GuildProfilePermissions.Add(new EF.Models.User_GuildProfilePermissions()
                    {
                        PermissionLevelId = (int)EF.Models.Enums.GuildProfilePermissionLevel.Admin,
                        ProfileId = newProfile.Id,
                        UserId = creatorId
                    });
                }

                await this.context.SaveChangesAsync();

                transaction.Commit();

                return newProfile.Id;
            }
        }

        public async Task<IEnumerable<EfBlizzardModels.StoredPlayer>> InsertGuildPlayersIfNeededAsync(
            IEnumerable<EfBlizzardModels.StoredPlayer> players, 
            int profileId,
            int guildId)
        {
            var realmIds = players.Select(x => x.RealmId).Distinct();
            var playerNames = players.Select(x => x.Name).Distinct();

            var existingStoredPlayers = await this.context.StoredPlayers
                .Where(x => x.ProfileId == profileId && playerNames.Contains(x.Name))
                .AsNoTracking()
                .ToListAsync();

            var notFoundStoredPlayers = players
                .Where(
                    x => !existingStoredPlayers.Any(
                        y => x.Name == y.Name && x.RealmId == y.RealmId));

            foreach (var notFoundPlayer in notFoundStoredPlayers)
            {
                this.context.StoredPlayers.Add(notFoundPlayer);
            }

            await this.context.SaveChangesAsync();

            var finalValues = await this
                .context
                .StoredPlayers
                .Include(x => x.Realm)
                    .ThenInclude(x => x.Region)
                .Include(x => x.Guild)
                .Where(x => x.ProfileId == profileId && x.GuildId == guildId)
                .AsNoTracking()
                .ToListAsync();

            return finalValues;
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

                if (result != null && result.ExpiresOn < DateTime.Now)
                {
                    this.context.BigValueCache.Remove(result);

                    await this.context.SaveChangesAsync();
                }

                this.context.BigValueCache.Add(new BigValueCache()
                {
                    Id = key,
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
                .Include(g => g.User_GuildProfilePermissions)
                .Where(g => g.CreatorId == userId || g.User_GuildProfilePermissions.Any(x => x.UserId == userId))
                .ToListAsync();
        }

        public async Task<EfEnums.GuildProfilePermissionLevel?> GetProfilePermissionForUserAsync(int profileId, string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return null;
            }

            var profile = await this.context.GuildProfile.FirstOrDefaultAsync(x => x.Id == profileId);

            var permission = await this.context.User_GuildProfilePermissions
                .Include(p => p.PermissionLevel)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.UserId == userId && p.ProfileId == profileId);

            if (permission == null)
            {
                return null;
            }

            return (EfEnums.GuildProfilePermissionLevel)permission.PermissionLevel.Id;
        }

        public async Task<bool> ProfileIsPublicAsync(int profileId)
        {
            var profile = await this.context.GuildProfile.FirstOrDefaultAsync(x => x.Id == profileId);
    
            if (profile == null)
            {
                throw new UserReportableError($"No profile found with ID {profileId}.", (int)HttpStatusCode.BadRequest);
            }

            return profile.IsPublic;
        }

        public async Task<IdentityUser> GetUserByEmailAddressAsync(string email)
        {
            return await this.context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<EfModels.GuildProfile> GetFullGuildProfileAsync(int profileId)
        {
            var profile = await this.context.GuildProfile
                .Include(x => x.Creator)
                .Include(x => x.CreatorGuild)
                    .ThenInclude(x => x.Realm)
                    .ThenInclude(x => x.Region)
                .Include(x => x.PlayerMains)
                    .ThenInclude(y => y.Alts)
                    .ThenInclude(z => z.Player)
                    .ThenInclude(a => a.Realm)
                    .ThenInclude(b => b.Region)
                .Include(x => x.PlayerMains)
                    .ThenInclude(y => y.Player)
                    .ThenInclude(z => z.Realm)
                    .ThenInclude(z => z.Region)
                .Include(x => x.Realm)
                    .ThenInclude(y => y.Region)
                .Include(x => x.User_GuildProfilePermissions)
                    .ThenInclude(y => y.PermissionLevel)
                .Include(x => x.AccessRequests)
                .Include(x => x.FriendGuilds)
                    .ThenInclude(x => x.Guild)
                    .ThenInclude(x => x.Realm)
                    .ThenInclude(x => x.Region)
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == profileId);

            return profile;
        }

        public async Task<GuildProfile> GetGuildProfile_GuildAndFriendGuildsAsync(int id)
        {
            var profile = await this.context.GuildProfile
                .Include(x => x.CreatorGuild)
                    .ThenInclude(x => x.Realm)
                    .ThenInclude(x => x.Region)
                .Include(x => x.FriendGuilds)
                    .ThenInclude(x => x.Guild)
                    .ThenInclude(x => x.Realm)
                    .ThenInclude(x => x.Region)
                .AsNoTracking()
                .SingleAsync(x => x.Id == id);

            return profile;
        }

        public async Task DeleteProfileAsync(int id)
        {
            var profileTask = this.context.GuildProfile.FirstOrDefaultAsync(p => p.Id == id);
            var playersTask = this.context.StoredPlayers.Where(x => x.ProfileId == id).ToListAsync();
            var guildsTask = this.context.StoredGuilds.Where(x => x.ProfileId == id).ToListAsync();
            var mainsTask = this.context.PlayerMains.Where(x => x.ProfileId == id).ToListAsync();
            var altsTask = this.context.PlayerAlts.Where(x => x.ProfileId == id).ToListAsync();
            var permissionsTask = this.context.User_GuildProfilePermissions.Where(x => x.ProfileId == id).ToListAsync();
            var friendGuildsTask = this.context.FriendGuilds.Where(x => x.ProfileId == id).ToListAsync();

            await Task.WhenAll(profileTask, playersTask, guildsTask, mainsTask, altsTask, permissionsTask, friendGuildsTask);

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

                this.context.FriendGuilds.RemoveRange(friendGuildsTask.Result);

                await this.context.SaveChangesAsync();

                transaction.Commit();
            }
        }

        public async Task<EfModels.PlayerMain> AddMainToProfileAsync(int playerId, int profileId)
        {
            var profileTask = this.context.GuildProfile
                .Include(x => x.PlayerMains)
                .Include(x => x.PlayerAlts)
                .Include(x => x.Players)
                .SingleOrDefaultAsync(x => x.Id == profileId);

            var playerTask = this.context.StoredPlayers
                .SingleOrDefaultAsync(x => x.Id == playerId);

            await Task.WhenAll(profileTask, playerTask);

            var profile = profileTask.Result;
            var player = playerTask.Result;

            if (profile == null)
            {
                throw new UserReportableError($"Profile {profileId} not found.", (int)HttpStatusCode.BadRequest);
            }

            if (player == null)
            {
                throw new UserReportableError($"Player {playerId} not found.", (int)HttpStatusCode.BadRequest);
            }

            if (!profile.Players.Any(x => x.Id == playerId))
            {
                throw new UserReportableError($"Player {playerId} does not belong to this profile.", (int)HttpStatusCode.BadRequest);
            }

            if (profile.PlayerMains.Any(x => x.PlayerId == playerId) || profile.PlayerAlts.Any(x => x.PlayerId == playerId))
            {
                throw new UserReportableError($"Player {playerId} is already assigned!.", (int)HttpStatusCode.BadRequest);
            }

            var newPlayer = new EfModels.PlayerMain()
            {
                ProfileId = profileId,
                PlayerId = playerId
            };

            profile.PlayerMains.Add(newPlayer);

            await this.context.SaveChangesAsync();

            await context.Entry(newPlayer).Reference(x => x.Player).LoadAsync();
            await context.Entry(newPlayer.Player).Reference(x => x.Realm).LoadAsync();
            await context.Entry(newPlayer.Player.Realm).Reference(x => x.Region).LoadAsync();

            return newPlayer;
        }

        public async Task<EfModels.PlayerAlt> AddAltToMainAsync(int playerId, int mainId, int profileId)
        {
            var profileTask = this.context.GuildProfile
                .Include(x => x.PlayerMains)
                .Include(x => x.PlayerAlts)
                .Include(x => x.Players)
                .SingleOrDefaultAsync(x => x.Id == profileId);

            var mainTask = this.context.PlayerMains
                .SingleOrDefaultAsync(x => x.Id == mainId);

            var playerTask = this.context.StoredPlayers
                .SingleOrDefaultAsync(x => x.Id == playerId);

            await Task.WhenAll(profileTask, mainTask, playerTask);

            var profile = profileTask.Result;
            var player = playerTask.Result;
            var main = mainTask.Result;

            if (profile == null)
            {
                throw new UserReportableError($"Profile {profileId} not found.", (int)HttpStatusCode.BadRequest);
            }

            if (player == null)
            {
                throw new UserReportableError($"Player {playerId} not found.", (int)HttpStatusCode.BadRequest);
            }

            if (main == null)
            {
                throw new UserReportableError($"Main {mainId} not found.", (int)HttpStatusCode.BadRequest);
            }

            if (!profile.Players.Any(x => x.Id == playerId))
            {
                throw new UserReportableError($"Player {playerId} does not belong to this profile.", (int)HttpStatusCode.BadRequest);
            }

            if (profile.PlayerMains.Any(x => x.PlayerId == playerId) || profile.PlayerAlts.Any(x => x.PlayerId == playerId))
            {
                throw new UserReportableError($"Player {playerId} is already assigned!.", (int)HttpStatusCode.BadRequest);
            }

            var newAlt = new EfModels.PlayerAlt()
            {
                ProfileId = profileId,
                PlayerId = playerId,
                PlayerMainId = mainId
            };

            this.context.PlayerAlts.Add(newAlt);

            await this.context.SaveChangesAsync();

            await context.Entry(newAlt).Reference(x => x.Player).LoadAsync();
            await context.Entry(newAlt.Player).Reference(x => x.Realm).LoadAsync();
            await context.Entry(newAlt.Player.Realm).Reference(x => x.Region).LoadAsync();

            return newAlt;
        }

        public async Task AddAccessRequestAsync(string userId, int profileId)
        {
            this.context.PendingAccessRequests.Add(new EfModels.PendingAccessRequest()
            {
                RequesterId = userId,
                CreatedOn = DateTime.Now,
                ProfileId = profileId
            });

            await this.context.SaveChangesAsync();
        }

        public async Task ApproveAccessRequest(int requestId)
        {
            var request = await this.context.PendingAccessRequests.SingleOrDefaultAsync(x => x.Id == requestId);

            if (request == null)
            {
                throw new UserReportableError("Unable to locate this access request.", (int)HttpStatusCode.BadRequest);
            }

            this.context.User_GuildProfilePermissions.Add(new User_GuildProfilePermissions()
            {
                PermissionLevelId = (int)EfEnums.GuildProfilePermissionLevel.Member,
                ProfileId = request.ProfileId,
                UserId = request.RequesterId
            });

            this.context.PendingAccessRequests.Remove(request);

            await this.context.SaveChangesAsync();
        }


        public async Task<IEnumerable<RepositoryModels.ProfilePermissionByUser>> GetFullProfilePermissions(string userId, int profileId)
        {
            var profile = await this.context
                .GuildProfile
                .Include(x => x.User_GuildProfilePermissions)
                    .ThenInclude(x => x.User)
                .Include(x => x.User_GuildProfilePermissions)
                .FirstOrDefaultAsync(x => x.Id == profileId);

            return profile.User_GuildProfilePermissions.Select(x => new RepositoryModels.ProfilePermissionByUser()
            {
                User = x.User,
                PermissionLevel = (EfEnums.GuildProfilePermissionLevel)x.PermissionLevelId
            });
        }

        public async Task UpdatePermissions(string userId, IEnumerable<UpdatePermission> newPermissions, int profileId, bool isAdmin)
        {
            var activePermission = await this.GetProfilePermissionForUserAsync(profileId, userId);

            var newPermissionsUserIds = newPermissions.Select(x => x.UserId);
            var newPermissionsUsersList = await this.context.UserData.Where(x => newPermissionsUserIds.Contains(x.Id)).AsNoTracking().ToListAsync();
            var newPermissionsUsers = newPermissionsUsersList.ToDictionary(
                value => value.Id, value => value);
            var notFoundUsers = newPermissions.Where(desired => !newPermissionsUsers.ContainsKey(desired.UserId)).Select(x => x.UserId);

            if (notFoundUsers.Any())
            {
                throw new UserReportableError($"Could not locate users with the following ID's: { string.Join(", ", notFoundUsers)}", 
                    (int)HttpStatusCode.BadRequest);
            }

            var taskList = new List<Task>();

            using (var transaction = await this.context.Database.BeginTransactionAsync())
            {
                foreach (UpdatePermission permission in newPermissions)
                {
                    var targetUser = newPermissionsUsers[permission.UserId];

                    taskList.Add(UpdateSinglePermission(userId, activePermission.Value, permission, targetUser, profileId, isAdmin));
                }

                await Task.WhenAll(taskList.ToArray());

                transaction.Commit();
            }
        }

        public async Task<IEnumerable<EfModels.FriendGuild>> GetFriendGuilds(int profileId)
        {
            return await this.context.FriendGuilds
                .Include(x => x.Guild)
                    .ThenInclude(x => x.Realm)
                    .ThenInclude(x => x.Region)
                .Where(x => x.ProfileId == profileId)
                .ToListAsync();
        }

        public async Task<EfModels.FriendGuild> AddFriendGuild(int profileId, EfModels.StoredBlizzardModels.StoredGuild storedGuild)
        {
            var profile = await this.context.GuildProfile
                .Include(x => x.FriendGuilds)
                .FirstOrDefaultAsync(x => x.Id == profileId);

            if (profile.CreatorGuildId == storedGuild.Id)
            {
                throw new UserReportableError("Guild profile is already targeting this guild.", (int)HttpStatusCode.BadRequest);
            }

            if (profile.FriendGuilds.Any(x => x.StoredGuildId == storedGuild.Id))
            {
                throw new UserReportableError("This guild is already associated as a friend guild.", (int)HttpStatusCode.BadRequest);
            }

            string abbreviation = storedGuild.Name.Substring(0, Math.Min(storedGuild.Name.Length, 3)).ToUpper();

            var newFriendGuild = new EfModels.FriendGuild()
            {
                ProfileId = profileId,
                StoredGuildId = storedGuild.Id
            };

            storedGuild.Abbreviation = abbreviation;

            this.context.FriendGuilds.Add(newFriendGuild);

            await this.context.SaveChangesAsync();

            return await this.context.FriendGuilds
                .Include(x => x.Guild)
                    .ThenInclude(x => x.Realm)
                    .ThenInclude(x => x.Region)
                .SingleAsync(x => x.Id == newFriendGuild.Id);
        }

        public async Task DeleteFriendGuildAsync(int profileId, int friendGuildId)
        {
            var profile = await this.context.GuildProfile
                .Include(x => x.FriendGuilds)
                .FirstOrDefaultAsync(x => x.Id == profileId);

            var friendGuild = profile.FriendGuilds.Single(x => x.Id == friendGuildId);

            profile.FriendGuilds.Remove(friendGuild);
            this.context.FriendGuilds.Remove(friendGuild);

            await this.context.SaveChangesAsync();
        }

        public async Task AddNotification(string email, string operationKey, EfEnums.NotificationRequestTypeEnum type)
        {
            this.context.NotificationRequests.Add(new NotificationRequest()
            {
                Email = email,
                NotificationRequestTypeId = (int)type,
                OperationKey = operationKey
            });

            await this.context.SaveChangesAsync();
        }

        public async Task<IEnumerable<NotificationRequest>> GetAndClearNotifications(EfEnums.NotificationRequestTypeEnum type, string operationKey = null)
        {
            var query = this.context.NotificationRequests
                .AsNoTracking()
                .Where(x => x.NotificationRequestTypeId == (int)type);

            if (operationKey != null)
            {
                query = query.Where(x => x.OperationKey == operationKey);
            }

            var results = await query.ToListAsync();

            this.context.NotificationRequests.RemoveRange(results);

            await this.context.SaveChangesAsync();

            return results;
        }

        private async Task UpdateSinglePermission(
            string originatingUserId,
            EfEnums.GuildProfilePermissionLevel activeLevel, 
            UpdatePermission newPermission, 
            UserWithData targetUser,
            int profileId,
            bool isAdmin)
        {
            if (newPermission.Delete)
            {
                await this.UpdateSinglePermissionDelete(originatingUserId, activeLevel, targetUser, profileId, isAdmin);
            }
            else
            {
                await this.UpdateSinglePermissionUpdate(originatingUserId, activeLevel, newPermission, targetUser, profileId, isAdmin);
            }
        }

        private async Task UpdateSinglePermissionDelete(
            string originatingUserId,
            EfEnums.GuildProfilePermissionLevel activeLevel,
            UserWithData targetUser,
            int profileId,
            bool isAdmin)
        {
            int activeUserPermissionOrder = PermissionsOrder.Order(activeLevel);

            if (originatingUserId == targetUser.Id)
            {
                throw new UserReportableError($"User may not delete their own permissions.", (int)HttpStatusCode.BadRequest);
            }

            var targetUserLevel = await this.GetProfilePermissionForUserAsync(profileId, targetUser.Id);
            if (targetUserLevel == null)
            {
                throw new UserReportableError($"User {targetUser.Id} has not requested access!", (int)HttpStatusCode.BadRequest);
            }

            int targetUserOrder = PermissionsOrder.Order(targetUserLevel.Value);

            if (!isAdmin)
            {
                if (activeUserPermissionOrder < PermissionsOrder.Order(EfEnums.GuildProfilePermissionLevel.Officer))
                {
                    throw new UserReportableError("User does not have permissions to delete a user.", (int)HttpStatusCode.Unauthorized);
                }

                if (activeUserPermissionOrder < targetUserOrder)
                {
                    throw new UserReportableError("User can't delete another user with a higher permission level.", (int)HttpStatusCode.Unauthorized);
                }
            }

            var targetPermission = await this.context.User_GuildProfilePermissions.SingleOrDefaultAsync(
                x => x.ProfileId == profileId && x.UserId == targetUser.Id);

            this.context.User_GuildProfilePermissions.Remove(targetPermission);

            await this.context.SaveChangesAsync();
        }

        private async Task UpdateSinglePermissionUpdate(
            string originatingUserId,
            EfEnums.GuildProfilePermissionLevel activeLevel,
            UpdatePermission newPermission,
            UserWithData targetUser,
            int profileId, 
            bool isAdmin)
        {
            int newPermissionOrder = PermissionsOrder.Order((EfEnums.GuildProfilePermissionLevel)newPermission.NewPermissionLevel);
            int activeUserPermissionOrder = PermissionsOrder.Order(activeLevel);

            if (originatingUserId == targetUser.Id)
            {
                throw new UserReportableError($"User may not modify their own permissions.", (int)HttpStatusCode.BadRequest);
            }

            var targetUserLevel = await this.GetProfilePermissionForUserAsync(profileId, targetUser.Id);
            if (targetUserLevel == null)
            {
                throw new UserReportableError($"User {targetUser.Id} has not requested access!", (int)HttpStatusCode.BadRequest);
            }

            if (!isAdmin)
            {
                int targetUserOrder = PermissionsOrder.Order(targetUserLevel.Value);

                if (newPermissionOrder > PermissionsOrder.Order(activeLevel))
                {
                    throw new UserReportableError("User can't adjust permissions beyond their own level.", (int)HttpStatusCode.BadRequest);
                }

                if (PermissionsOrder.Order(activeLevel) < PermissionsOrder.Order(targetUserLevel.Value))
                {
                    throw new UserReportableError($"Can't adjust permissions on user with a higher permission level.",
                        (int)HttpStatusCode.BadRequest);
                }
            }

            var targetPermission = await this.context.User_GuildProfilePermissions.FirstOrDefaultAsync(
                x => x.ProfileId == profileId && x.UserId == targetUser.Id);

            targetPermission.PermissionLevelId = newPermission.NewPermissionLevel;

            await this.context.SaveChangesAsync();
        }

        public async Task<IEnumerable<EfModels.PendingAccessRequest>> GetAccessRequestsAsync(int profileId)
        {
            return await this.context
                .PendingAccessRequests
                .Include(x => x.Requester)
                .Where(x => x.ProfileId == profileId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task RemoveAltFromMainAsync(int mainId, int altId, int profileId)
        {
            var mainTask = this.context.PlayerMains.Include(p => p.Alts)
                .SingleOrDefaultAsync(x => x.Id == mainId);

            var altTask = this.context.PlayerAlts.Include(p => p.PlayerMain)
                .SingleOrDefaultAsync(x => x.Id == altId);

            await Task.WhenAll(mainTask, mainTask);

            if (mainTask.Result == null)
            {
                throw new UserReportableError($"No main player found with ID of '{mainId}'.", (int)HttpStatusCode.BadRequest);
            }

            if (altTask.Result == null)
            {
                throw new UserReportableError($"No alt player found with ID of '{altId}'.", (int)HttpStatusCode.BadRequest);
            }

            if (mainTask.Result.ProfileId != profileId || altTask.Result.ProfileId != profileId)
            {
                throw new UserReportableError($"Can only perform operations on characters attached to profile.", (int)HttpStatusCode.BadRequest);
            }

            if (!mainTask.Result.Alts.Any(x => x.Id == altId))
            {
                throw new UserReportableError($"Main player '{mainId}' does not own alt '{altId}'.", (int)HttpStatusCode.BadRequest);
            }

            mainTask.Result.Alts.Remove(altTask.Result);
            this.context.PlayerAlts.Remove(altTask.Result);

            await this.context.SaveChangesAsync();
        }

        public async Task<EfModels.PlayerMain> PromoteAltToMainAsync(int altId, int profileId)
        {
            var oldAlt = await this.context.PlayerAlts
                .SingleOrDefaultAsync(x => x.Id == altId);

            if (oldAlt == null)
            {
                throw new UserReportableError($"No alt player found with ID of '{altId}'.", (int)HttpStatusCode.BadRequest);
            }

            if (oldAlt.ProfileId != profileId)
            {
                throw new UserReportableError($"Can only perform operations on characters attached to profile.", (int)HttpStatusCode.BadRequest);
            }

            var oldMain = await this.context
                .PlayerMains
                .Include(x => x.Alts)
                    .ThenInclude(x => x.Player)
                    .ThenInclude(x => x.Realm)
                    .ThenInclude(x => x.Region)
                .Include(x => x.Player)
                    .ThenInclude(x => x.Realm)
                    .ThenInclude(x => x.Region)
                .FirstOrDefaultAsync(x => x.Id == oldAlt.PlayerMainId);

            using (var transaction = await this.context.Database.BeginTransactionAsync())
            {
                int originalAltPlayerId = oldAlt.PlayerId;
                oldAlt.PlayerId = oldMain.PlayerId.Value;
                oldMain.PlayerId = originalAltPlayerId;

                await this.context.SaveChangesAsync();

                transaction.Commit();

                return oldMain;
            }
        }

        public async Task RemoveMainAsync(int mainId, int profileId)
        {
            var mainTask = this.context.PlayerMains.Include(p => p.Alts)
                .SingleOrDefaultAsync(x => x.Id == mainId);

            var altsTask = this.context.PlayerAlts
                .Where(x => x.PlayerMainId == mainId)
                .ToListAsync();

            await Task.WhenAll(mainTask, altsTask);

            if (mainTask.Result == null)
            {
                throw new UserReportableError($"No main player found with ID of '{mainId}'.", (int)HttpStatusCode.BadRequest);
            }

            this.context.PlayerAlts.RemoveRange(altsTask.Result);
            this.context.PlayerMains.Remove(mainTask.Result);

            await this.context.SaveChangesAsync();
        }

        public async Task EditPlayerNotes(int profileId, int playerMainId, string newNotes)
        {
            var playerMain = await this.context.PlayerMains.FirstOrDefaultAsync(x => x.Id == playerMainId);

            if (playerMain.ProfileId != profileId)
            {
                throw new UserReportableError("Player does not belong to target profile!", (int)HttpStatusCode.BadRequest);
            }

            playerMain.Notes = newNotes;

            await this.context.SaveChangesAsync();
        }

        public async Task EditOfficerNotes(int profileId, int playerMainId, string newNotes)
        {
            var playerMain = await this.context.PlayerMains.FirstOrDefaultAsync(x => x.Id == playerMainId);

            if (playerMain.ProfileId != profileId)
            {
                throw new UserReportableError("Player does not belong to target profile!", (int)HttpStatusCode.BadRequest);
            }

            playerMain.OfficerNotes = newNotes;

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

        public void Dispose()
        {
            this.context.Dispose();
        }
    }
}
