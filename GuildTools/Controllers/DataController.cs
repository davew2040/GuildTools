using GuildTools.Cache;
using GuildTools.Cache.SpecificCaches;
using GuildTools.Cache.SpecificCaches.CacheInterfaces;
using GuildTools.Controllers.InputModels;
using GuildTools.Controllers.JsonResponses;
using GuildTools.Controllers.Models;
using GuildTools.Data;
using GuildTools.EF.Models.Enums;
using GuildTools.ErrorHandling;
using GuildTools.ExternalServices.Blizzard;
using GuildTools.Permissions;
using GuildTools.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static GuildTools.EF.Models.Enums.EnumUtilities;
using static GuildTools.ExternalServices.Blizzard.BlizzardService;
using EfModels = GuildTools.EF.Models;
using EfEnums = GuildTools.EF.Models.Enums;
using EfBlizzardModels = GuildTools.EF.Models.StoredBlizzardModels;
using GuildTools.Cache.LongRunningRetrievers;
using GuildTools.Utilities;
using GuildTools.Cache.LongRunningRetrievers.Interfaces;

namespace GuildTools.Controllers
{
    [Route("api/[controller]")]
    public class DataController : GuildToolsController
    {
        private readonly ConnectionStrings connectionStrings;
        private readonly IBlizzardService blizzardService;
        private readonly IGuildStatsRetriever guildStatsRetriever;
        private readonly IRaiderIoStatsRetriever raiderIoStatsRetriever;
        private readonly IDataRepository dataRepo;
        private readonly IRealmsCache realmsCache;
        private readonly IGuildCache guildCache;
        private readonly IPlayerCache playerCache;
        private readonly IGuildMemberCache guildMemberCache;
        private readonly IGuildService guildService;
        private readonly IGuildStoreByName guildStore;
        private readonly IPlayerStoreByValue playerStore;
        private readonly IRealmStoreByValues realmStoreByValues;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UserManager<EfModels.UserWithData> userManager;

        private static bool sent = false;

        public DataController(
            IOptions<ConnectionStrings> connectionStrings,
            IDataRepository repository,
            IGuildService guildService,
            IBlizzardService blizzardService,
            IGuildStatsRetriever guildStatsRetriever,
            IRaiderIoStatsRetriever raiderIoStatsRetriever,
            IGuildCache guildCache,
            IPlayerCache playerCache,
            IGuildMemberCache guildMemberCache,
            IRealmsCache realmsCache,
            IGuildStoreByName guildStore,
            IPlayerStoreByValue playerStore,
            IRealmStoreByValues realmStoreByValues,
            RoleManager<IdentityRole> roleManager,
            UserManager<EfModels.UserWithData> userManager)
        {
            this.connectionStrings = connectionStrings.Value;
            this.blizzardService = blizzardService;
            this.guildStatsRetriever = guildStatsRetriever;
            this.raiderIoStatsRetriever = raiderIoStatsRetriever;
            this.guildService = guildService;
            this.dataRepo = repository;
            this.guildCache = guildCache;
            this.playerCache = playerCache;
            this.guildMemberCache = guildMemberCache;
            this.realmsCache = realmsCache;
            this.playerStore = playerStore;
            this.guildStore = guildStore;
            this.realmStoreByValues = realmStoreByValues;

            this.roleManager = roleManager;
            this.userManager = userManager;
        }

        [HttpGet]
        [Route("public")]
        public IActionResult Public()
        {
            return Json(new
            {
                Message = "Hello from a public endpoint! You don't need to be authenticated to see this."
            });
        }

        [Authorize]
        [HttpGet]
        [Route("private")]
        public IActionResult Private()
        {
            return Json(new
            {
                Message = "Hello from a private endpoint! You need to be authenticated to see this."
            });
        }

        [HttpGet("getGuildMemberStats")]
        public async Task<GuildStatsResponse> GetGuildMemberStats(string region, string guild, string realm)
        {
            guild = BlizzardService.FormatGuildName(guild);
            realm = BlizzardService.FormatRealmName(realm);
            BlizzardRegion regionEnum = BlizzardService.GetRegionFromString(region);

            var guildData = await this.guildStatsRetriever.GetCachedEntry(regionEnum, realm, guild, this.GetBaseUrl());

            if (guildData.State == CachedValueState.Updating)
            {
                string key = this.guildStatsRetriever.GetKey(regionEnum, realm, guild);
                var positionInQueue = this.guildStatsRetriever.GetPositionInQueue(key);

                return new GuildStatsResponse()
                {
                    IsCompleted = false,
                    PositionInQueue = positionInQueue,
                    CompletionProgress = guildData.CompletionProgress
                };
            }
            else
            {
                return new GuildStatsResponse()
                {
                    IsCompleted = true,
                    Values = guildData.Value
                };
            }
        }

        [HttpGet("getRaiderIoStats")]
        public async Task<RaiderIoStatsResponse> GetRaiderIoStats(string region, string guild, string realm)
        {
            guild = BlizzardService.FormatGuildName(guild);
            realm = BlizzardService.FormatRealmName(realm);
            BlizzardRegion regionEnum = BlizzardService.GetRegionFromString(region);

            var guildData = await this.raiderIoStatsRetriever.GetCachedEntry(regionEnum, realm, guild, this.GetBaseUrl());

            if (guildData.State == CachedValueState.Updating)
            {
                string key = this.raiderIoStatsRetriever.GetKey(regionEnum, realm, guild);
                var positionInQueue = this.raiderIoStatsRetriever.GetPositionInQueue(key);

                return new RaiderIoStatsResponse()
                {
                    IsCompleted = false,
                    PositionInQueue = positionInQueue,
                    CompletionProgress = guildData.CompletionProgress
                };
            }
            else
            {
                return new RaiderIoStatsResponse()
                {
                    IsCompleted = true,
                    Values = guildData.Value
                };
            }
        }

        [HttpPost("requestStatsCompleteNotification")]
        public async Task RequestStatsCompleteNotification([FromBody] RequestStatsCompleteNotification input)
        {
            input.Guild = BlizzardService.FormatGuildName(input.Guild);
            input.Realm = BlizzardService.FormatRealmName(input.Realm);
            BlizzardRegion regionEnum = BlizzardService.GetRegionFromString(input.Region);
            
            if (!RegexUtilities.IsValidEmail(input.Email))
            {
                throw new UserReportableError("Email is not in a valid format.", (int)HttpStatusCode.BadRequest);
            }

            var key = this.guildStatsRetriever.GetKey(regionEnum, input.Realm, input.Guild);

            await this.dataRepo.AddNotification(input.Email, key, (NotificationRequestTypeEnum)input.RequestType);
        }

        [HttpGet("guildExists")]
        public async Task<GuildFound> GuildExists(string region, string guild, string realm)
        {
            InputValidators.ValidateGuildName(guild);
            InputValidators.ValidateRealmName(realm);

            guild = BlizzardService.FormatGuildName(guild);
            realm = BlizzardService.FormatRealmName(realm);
            var regionEnum = GameRegionUtilities.GetGameRegionFromString(region);

            var locatedGuild = await this.guildCache.GetGuild(regionEnum, guild, realm);

            if (locatedGuild == null)
            {
                return new GuildFound()
                {
                    Found = false
                };
            }

            return new GuildFound()
            {
                Found = true,
                RealmName = locatedGuild.Realm,
                GuildName = locatedGuild.Name
            };
        }

        [HttpGet("playerExists")]
        public async Task<PlayerFound> PlayerExists(string region, string playerName, string realm)
        {
            InputValidators.ValidateRealmName(realm);
            InputValidators.ValidatePlayerName(playerName);

            realm = BlizzardService.FormatRealmName(realm);
            var regionEnum = GameRegionUtilities.GetGameRegionFromString(region);

            var locatedPlayer = await this.playerCache.GetPlayer(regionEnum, playerName, realm);

            if (locatedPlayer == null)
            {
                return new PlayerFound()
                {
                    Found = false
                };
            }

            return new PlayerFound()
            {
                Found = true,
                PlayerDetails = locatedPlayer
            };
        }

        [HttpPost("createGuildProfile")]
        public async Task<int> CreateGuildProfile([FromBody] CreateNewGuildProfile input)
        {
            InputValidators.ValidateProfileName(input.ProfileName);
            InputValidators.ValidateGuildName(input.GuildName);
            InputValidators.ValidateRealmName(input.GuildRealmName);

            input.GuildName = BlizzardService.FormatGuildName(input.GuildName);
            input.GuildRealmName = BlizzardService.FormatRealmName(input.GuildRealmName);
            var regionEnum = GameRegionUtilities.GetGameRegionFromString(input.RegionName);

            var user = await this.userManager.GetUserAsync(HttpContext.User);

            if (user == null && !input.IsPublic)
            {
                throw new UserReportableError("Must be authenticated to create a private profile.", (int)HttpStatusCode.Unauthorized);
            }

            var locatedRealm = await this.realmStoreByValues.GetRealmAsync(input.GuildRealmName, regionEnum);
            if (locatedRealm == null)
            {
                throw new UserReportableError($"Couldn't locate realm '{input.GuildRealmName}'.", 404);
            }

            var locatedGuild = await this.guildCache.GetGuild(regionEnum, input.GuildName, input.GuildRealmName);

            if (locatedGuild == null)
            {
                throw new UserReportableError("Could not locate this guild.", 404);
            }

            return await this.dataRepo.CreateGuildProfileAsync(user?.Id, input.ProfileName, locatedGuild, locatedRealm, regionEnum, input.IsPublic);
        }

        [HttpPost("addMainToProfile")]
        public async Task<PlayerMain> AddMainToProfile([FromBody] AddMainToProfile input)
        {
            var user = await this.userManager.GetUserAsync(HttpContext.User);

            if (!await this.UserCanAddMainAsync(user, input.ProfileId))
            {
                throw new UserReportableError("This user doesn't have permissions to perform this operation.", 401);
            }

            var newPlayer = await this.dataRepo.AddMainToProfileAsync(input.PlayerId, input.ProfileId);

            return new PlayerMain()
            {
                Id = newPlayer.Id,
                Notes = newPlayer.Notes,
                Player = this.MapPlayer(newPlayer.Player)
            };
        }

        [HttpPost("addAltToMain")]
        public async Task<PlayerAlt> AddAltToMain([FromBody] AddAltToMain input)
        {
            var user = await this.userManager.GetUserAsync(HttpContext.User);

            if (!await this.UserCanAddAltAsync(user, input.ProfileId))
            {
                throw new UserReportableError("This user doesn't have permissions to perform this operation.", 401);
            }

            var newAlt = await this.dataRepo.AddAltToMainAsync(input.PlayerId, input.MainId, input.ProfileId);

            return new PlayerAlt()
            {
                Id = newAlt.Id,
                Player = this.MapPlayer(newAlt.Player)
            };
        }

        [HttpPost("removeAltFromMain")]
        public async Task RemoveAltFromMain([FromBody] RemoveAltFromMain input)
        {
            var user = await this.userManager.GetUserAsync(HttpContext.User);

            if (!await this.UserCanRemoveAltAsync(user, input.ProfileId))
            {
                throw new UserReportableError("This user doesn't have permissions to perform this operation.", 401);
            }

            await this.dataRepo.RemoveAltFromMainAsync(input.MainId, input.AltId, input.ProfileId);
        }

        [HttpPost("promoteAltToMain")]
        public async Task<PlayerMain> PromoteAltToMain([FromBody] PromoteAltToMain input)
        {
            var user = await this.userManager.GetUserAsync(HttpContext.User);

            if (!await this.UserCanPromoteAltsAsync(user, input.ProfileId))
            {
                throw new UserReportableError("This user doesn't have permissions to perform this operation.", 401);
            }

            var permissionLevel = await this.GetCurrentPermissionLevel(input.ProfileId, user);
            bool isOfficer = permissionLevel.HasValue
                ? PermissionsOrder.GreaterThanOrEqual(permissionLevel.Value,
                    GuildProfilePermissionLevel.Officer)
                : false;

            var newMain = await this.dataRepo.PromoteAltToMainAsync(input.AltId, input.ProfileId);

            return this.MapPlayerMain(newMain, isOfficer);
        }

        [HttpPost("removeMain")]
        public async Task RemoveMain([FromBody] RemoveMain input)
        {
            var user = await this.userManager.GetUserAsync(HttpContext.User);

            if (!await this.UserCanRemoveMain(user, input.ProfileId))
            {
                throw new UserReportableError("This user doesn't have permissions to perform this operation.", 401);
            }

            await this.dataRepo.RemoveMainAsync(input.MainId, input.ProfileId);
        }


        [Authorize]
        [HttpGet("getGuildProfiles")]
        public async Task<IEnumerable<GuildProfile>> GetGuildProfiles()
        {
            var user = await this.userManager.GetUserAsync(HttpContext.User);

            if (user == null)
            {
                return new List<GuildProfile>();
            }

            var profiles = await this.dataRepo.GetGuildProfilesForUserAsync(user.Id);

            var jsonProfiles = profiles.Select(p => new GuildProfile()
            {
                Id = p.Id,
                ProfileName = p.ProfileName,
                GuildName = p.CreatorGuild.Name,
                Realm = p.Realm.Name,
                Region = p.Realm.Region.RegionName,
                Creator = new UserStub()
                {
                    Id = p.Creator.Id,
                    Email = p.Creator.Email,
                    Username = p.Creator.UserName
                }, 
                IsPublic = p.IsPublic
            });

            return jsonProfiles;
        }

        [HttpGet("getGuildProfile")]
        public async Task<FullGuildProfile> GetFullGuildProfile(int profileId)
        {
            var user = await this.userManager.GetUserAsync(HttpContext.User);

            var permissionLevel = await this.GetCurrentPermissionLevel(profileId, user);
            bool isOfficer = permissionLevel.HasValue
                ? PermissionsOrder.GreaterThanOrEqual(permissionLevel.Value,
                    GuildProfilePermissionLevel.Officer)
                : false;

            var repoProfile = await this.dataRepo.GetFullGuildProfileAsync(profileId);
            var efProfile = repoProfile;

            var returnProfile = new FullGuildProfile()
            {
                Id = efProfile.Id,
                ProfileName = efProfile.ProfileName,
                Creator = new UserStub()
                {
                    Id = efProfile.Creator?.Id,
                    Email = efProfile.Creator?.Email,
                    Username = efProfile.Creator?.UserName
                },
                GuildName = efProfile.CreatorGuild?.Name,
                Mains = efProfile.PlayerMains.Select(x => this.MapPlayerMain(x, isOfficer)),
                Realm = new StoredRealm()
                {
                    Id = efProfile.Realm.Id,
                    Name = efProfile.Realm.Name,
                    RegionId = efProfile.Realm.Region.Id
                },
                Region = efProfile.Realm.Region.RegionName,
                IsPublic = efProfile.IsPublic,
                AccessRequestCount = isOfficer ? efProfile.AccessRequests.Count() : 0
            };

            returnProfile.Players = (await this.GetOrInsertProfileGuildMembers(
                    profileId,
                    efProfile.CreatorGuild,
                    efProfile.Realm,
                    (EfEnums.GameRegionEnum)efProfile.Realm.RegionId))
                .Select(x => this.MapPlayer(x));

            returnProfile.CurrentPermissionLevel = (int?)permissionLevel;

            return returnProfile;
        }

        private async Task<IEnumerable<EfBlizzardModels.StoredPlayer>> GetOrInsertProfileGuildMembers(int profileId, EfBlizzardModels.StoredGuild guild, EfBlizzardModels.StoredRealm realm, EfEnums.GameRegionEnum region)
        {
            var members = await this.guildMemberCache.GetMembers(region, realm.Name, guild.Name);

            var realmNames = members.Select(x => x.PlayerRealmName).Distinct();

            var realmsTasks = realmNames.Select(async x =>  await this.realmStoreByValues.GetRealmAsync(x, region));
            await Task.WhenAll(realmsTasks);

            var realms = realmsTasks.Select(x => x.Result);

            var newPlayers = await this.dataRepo.InsertPlayersIfNeededAsync(members.Select(x =>
                new EfBlizzardModels.StoredPlayer()
                {
                    Name = x.PlayerName,
                    Class = x.Class,
                    Level = x.Level,
                    GuildId = guild.Id,
                    RealmId = realms.SingleOrDefault(y => y.Name == x.PlayerRealmName).Id,
                    ProfileId = profileId
                }), profileId);

            return newPlayers;
        }

        [HttpDelete("deleteProfile")]
        public async Task DeleteProfile(int profileId)
        {
            var user = await this.userManager.GetUserAsync(HttpContext.User);

            if (!await this.CanDeleteProfile(user, profileId))
            {
                throw new UserReportableError("This user doesn't have permissions to perform this operation.", (int)HttpStatusCode.Unauthorized);
            }

            await this.dataRepo.DeleteProfileAsync(profileId);
        }

        [Authorize]
        [HttpPost("addAccessRequest")]
        public async Task AddAccessRequest(int profileId)
        {
            var user = await this.userManager.GetUserAsync(HttpContext.User);

            if (await this.dataRepo.GetProfilePermissionForUserAsync(profileId, user.Id) != null)
            {
                throw new UserReportableError($"This user already has permissions for profile '{profileId}'.", (int)HttpStatusCode.BadRequest);
            }

            var existingAccessRequests = await this.dataRepo.GetAccessRequestsAsync(profileId);

            if (existingAccessRequests.Any(x => x.RequesterId == user.Id))
            {
                throw new UserReportableError($"User has already submitted an access request for profile '{profileId}'.", (int)HttpStatusCode.BadRequest);
            }

            await this.dataRepo.AddAccessRequestAsync(user.Id, profileId);
        }

        [Authorize]
        [HttpPost("approveAccessRequest")]
        public async Task ApproveAccessRequest(int requestId)
        {
            var user = await this.userManager.GetUserAsync(HttpContext.User);

            if (!(await this.UserCanApproveAccessRequest(user, requestId)))
            {
                throw new UserReportableError($"This user does not have permission to approve access requests for profile '{requestId}'.",
                    (int)HttpStatusCode.Unauthorized);
            }

            await this.dataRepo.ApproveAccessRequest(requestId);
        }

        [Authorize]
        [HttpGet("getAccessRequests")]
        public async Task<IEnumerable<PendingAccessRequest>> GetAccessRequests(int profileId)
        {
            var user = await this.userManager.GetUserAsync(HttpContext.User);

            var userPermissionLevel = await this.dataRepo.GetProfilePermissionForUserAsync(profileId, user.Id);
            if (!userPermissionLevel.HasValue 
                || !PermissionsOrder.GreaterThanOrEqual(userPermissionLevel.Value, GuildProfilePermissionLevel.Officer))
            {
                throw new UserReportableError($"User does not have permissions to approve access requests for profile '{profileId}'.", 
                    (int)HttpStatusCode.Unauthorized);
            }

            var efRequests = await this.dataRepo.GetAccessRequestsAsync(profileId);
            return efRequests.Select(x => new PendingAccessRequest()
            {
                Id = x.Id,
                CreatedOn = x.CreatedOn,
                ProfileId = x.ProfileId,
                User = new UserStub()
                {
                    Id = x.Requester.Id,
                    Email = x.Requester.Email,
                    Username = x.Requester.UserName
                }
            });
        }


        [Authorize]
        [HttpGet("getAllProfilePermissions")]
        public async Task<FullProfilePermissions> GetAllProfilePermissions(int profileId)
        {
            var user = await this.userManager.GetUserAsync(HttpContext.User);

            if (!(await this.CanGetAllProfilePermissions(user, profileId)))
            {
                throw new UserReportableError($"This user does not have permission to retrieve profile permissions for profile '{profileId}'.",
                    (int)HttpStatusCode.Unauthorized);
            }

            var userPermissionTask = this.dataRepo.GetProfilePermissionForUserAsync(profileId, user.Id);
            var allPermissionsTask = this.dataRepo.GetFullProfilePermissions(user.Id, profileId);

            await Task.WhenAll(userPermissionTask, allPermissionsTask);

            return new FullProfilePermissions()
            {
                CurrentPermissions = (int)userPermissionTask.Result.Value,
                Permissions = allPermissionsTask.Result.Select(x => new ProfilePermissionByUser()
                {
                    PermissionLevel = (int)x.PermissionLevel,
                    User = new UserStub()
                    {
                        Id = x.User.Id,
                        Email = x.User.Email,
                        Username = x.User.UserName
                    }
                })
            };
        }

        [Authorize]
        [HttpPost("updatePermissions")]
        public async Task UpdatePermissions([FromBody] UpdatePermissionSet updates)
        {
            var user = await this.userManager.GetUserAsync(HttpContext.User);

            if (!(await this.CanUpdatePermissions(user, updates.ProfileId)))
            {
                throw new UserReportableError($"This user does not have permission to update profile permissions for profile '{updates.ProfileId}'.",
                    (int)HttpStatusCode.Unauthorized);
            }

            bool isAdmin = await this.IsAdmin(user);

            await this.dataRepo.UpdatePermissions(user.Id, updates.Updates, updates.ProfileId, isAdmin);
        }


        [Authorize]
        [HttpPost("editPlayerNotes")]
        public async Task EditPlayerNotes([FromBody] EditNotes editDetails)
        {
            var user = await this.userManager.GetUserAsync(HttpContext.User);

            if (!(await this.UserCanEditNotes(editDetails.ProfileId, user)))
            {
                throw new UserReportableError($"This user does not have permission to edit player notes for profile '{editDetails.ProfileId}'.",
                    (int)HttpStatusCode.Unauthorized);
            }

            await this.dataRepo.EditPlayerNotes(editDetails.ProfileId, editDetails.PlayerMainId, editDetails.NewNotes);
        }

        [Authorize]
        [HttpPost("editOfficerNotes")]
        public async Task EditOfficerNotes([FromBody] EditNotes editDetails)
        {
            var user = await this.userManager.GetUserAsync(HttpContext.User);

            if (!(await this.UserCanEditNotes(editDetails.ProfileId, user)))
            {
                throw new UserReportableError($"This user does not have permission to edit officer notes for profile '{editDetails.ProfileId}'.",
                    (int)HttpStatusCode.Unauthorized);
            }

            await this.dataRepo.EditOfficerNotes(editDetails.ProfileId, editDetails.PlayerMainId, editDetails.NewNotes);
        }

        [HttpGet("getRealms")]
        public async Task<IEnumerable<Realm>> GetRealms(string region)
        {
            return await this.realmsCache.GetRealms(EnumUtilities.GameRegionUtilities.GetGameRegionFromString(region));
        }

        private async Task<bool> UserCanAddMainAsync(EfModels.UserWithData user, int profileId)
        {
            return await this.UserHasStandardOfficerPermissions(profileId, user);
        }

        private async Task<bool> UserCanAddAltAsync(EfModels.UserWithData user, int profileId)
        {
            return await this.UserHasStandardOfficerPermissions(profileId, user);
        }

        private async Task<bool> UserCanRemoveAltAsync(EfModels.UserWithData user, int profileId)
        {
            return await this.UserHasStandardOfficerPermissions(profileId, user);
        }

        private async Task<bool> UserCanPromoteAltsAsync(EfModels.UserWithData user, int profileId)
        {
            return await this.UserHasStandardOfficerPermissions(profileId, user);
        }

        private async Task<bool> UserCanRemoveMain(EfModels.UserWithData user, int profileId)
        {
            return await this.UserHasStandardOfficerPermissions(profileId, user);
        }

        private async Task<bool> CanDeleteProfile(EfModels.UserWithData user, int profileId)
        {
            return await this.UserHasStandardOfficerPermissions(profileId, user);
        }

        private async Task<bool> UserCanApproveAccessRequest(EfModels.UserWithData user, int profileId)
        {
            return await this.UserHasStandardOfficerPermissions(profileId, user);
        }

        private async Task<bool> CanGetAllProfilePermissions(EfModels.UserWithData user, int profileId)
        {
            return await this.UserHasStandardOfficerPermissions(profileId, user);
        }

        private async Task<bool> CanUpdatePermissions(EfModels.UserWithData user, int profileId)
        {
            return await this.UserHasStandardOfficerPermissions(profileId, user);
        }


        private async Task<bool> UserCanEditNotes(int profileId, EfModels.UserWithData user)
        {
            return await this.UserHasStandardOfficerPermissions(profileId, user);
        }

        private async Task<bool> UserHasStandardOfficerPermissions(int profileId, EfModels.UserWithData user)
        {
            if (await this.IsAdmin(user))
            {
                return true;
            }

            if (await this.dataRepo.ProfileIsPublicAsync(profileId))
            {
                return true;
            }

            var permissionsForProfile = await this.dataRepo.GetProfilePermissionForUserAsync(profileId, user?.Id);

            if (!permissionsForProfile.HasValue)
            {
                return false;
            }

            return PermissionsOrder.GetPermissionOrder(permissionsForProfile.Value)
                >= PermissionsOrder.GetPermissionOrder(GuildProfilePermissionLevel.Officer);
        }

        private async Task<GuildProfilePermissionLevel?> GetCurrentPermissionLevel(int profileId, EfModels.UserWithData user)
        {
            var isAdminTask = this.IsAdmin(user);
            var profilePermissionTask = this.dataRepo.GetProfilePermissionForUserAsync(profileId, user?.Id);

            await Task.WhenAll(isAdminTask, profilePermissionTask);

            if (isAdminTask.Result)
            {
                return GuildProfilePermissionLevel.Admin;
            }

            if (!profilePermissionTask.Result.HasValue)
            {
                return null;
            }

            return profilePermissionTask.Result.Value;
        }

        private async Task<bool> IsAdmin(EfModels.UserWithData user)
        {
            if (user == null)
            {
                return false;
            }

            var roles = await this.GetRolesForUser(user);
            return (roles.Any(r => r == GuildToolsRoles.AdminRole.Name));
        }

        private async Task<IEnumerable<string>> GetRolesForUser(EfModels.UserWithData user)
        {
            return (await this.userManager.GetRolesAsync(user));
        }

        private PlayerMain MapPlayerMain(EfModels.PlayerMain efMain, bool isOfficer)
        {
            if (efMain == null)
            {
                return null;
            }

            return new PlayerMain()
            {
                Id = efMain.Id,
                Notes = efMain.Notes,
                OfficerNotes = isOfficer ? efMain.OfficerNotes : string.Empty,
                Player = MapPlayer(efMain.Player),
                Alts = efMain.Alts.Select(x => MapPlayerAlt(x))
            };
        }

        private PlayerAlt MapPlayerAlt(EfModels.PlayerAlt efAlt)
        {
            if (efAlt == null)
            {
                return null;
            }

            return new PlayerAlt()
            {
                Id = efAlt.Id,
                Player = MapPlayer(efAlt.Player)
            };
        }

        private StoredPlayer MapPlayer(EfModels.StoredBlizzardModels.StoredPlayer efPlayer)
        {
            return new StoredPlayer()
            {
                Id = efPlayer.Id,
                Name = efPlayer.Name,
                Class = efPlayer.Class,
                Level = efPlayer.Level,
                Realm = this.MapRealm(efPlayer.Realm),
                RegionName = efPlayer.Realm.Region.RegionName
            };
        }

        private StoredRealm MapRealm(EfModels.StoredBlizzardModels.StoredRealm efRealm)
        {
            if (efRealm == null)
            {
                return null;
            }

            return new StoredRealm()
            {
                Id = efRealm.Id,
                Name = efRealm.Name,
                Slug = efRealm.Slug,
                RegionId = efRealm.RegionId
            };
        }

        private class InputValidators
        {
            public static bool ValidateProfileName(string profileName)
            {
                return Regex.IsMatch(profileName, @"^[a-zA-Z0-9'\s]+$");
            }

            public static bool ValidatePlayerName(string playerName)
            {
                return Regex.IsMatch(playerName, @"^[a-zA-Z0-9]+$");
            }

            public static bool ValidateGuildName(string guildName)
            {
                return Regex.IsMatch(guildName, @"^[a-zA-Z0-9\s]+$");
            }

            public static bool ValidateRealmName(string guildName)
            {
                return Regex.IsMatch(guildName, @"^[a-zA-Z0-9\s']+$");
            }

            public static void ThrowOnValidateProfileName(string profileName)
            {
                if (!ValidateProfileName(profileName))
                {
                    throw new ArgumentException("Profile Name is invalid.");
                }
            }

            public static void ThrowOnValidatePlayerName(string playerName)
            {
                if (!ValidatePlayerName(playerName))
                {
                    throw new ArgumentException("Player name is invalid.");
                }
            }

            public static void ThrowOnValidateGuildName(string guildName)
            {
                if (!ValidateGuildName(guildName))
                {
                    throw new ArgumentException("Guild name is invalid.");
                }
            }

            public static void ThrowOnValidateRealmName(string realmName)
            {
                if (!ValidateRealmName(realmName))
                {
                    throw new ArgumentException("Realm name is invalid.");
                }
            }
        }
    }
}
