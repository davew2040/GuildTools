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

namespace GuildTools.Controllers
{
    [Route("api/[controller]")]
    public class DataController : Controller
    {
        private readonly ConnectionStrings connectionStrings;
        private readonly IBlizzardService blizzardService;
        private readonly IOldGuildMemberCache guildMemberStatsCache;
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

        public DataController(
            IOptions<ConnectionStrings> connectionStrings,
            IDataRepository repository,
            IGuildService guildService,
            IBlizzardService blizzardService,
            IOldGuildMemberCache oldGuildMemberCache,
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
            this.guildMemberStatsCache = oldGuildMemberCache;
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
        public async Task<IEnumerable<GuildMemberStats>> GetGuildMemberStats(string region, string guild, string realm)
        {
            guild = BlizzardService.FormatGuildName(guild);
            realm = BlizzardService.FormatRealmName(realm);
            BlizzardRegion regionEnum = BlizzardService.GetRegionFromString(region);

            var guildData = await this.guildMemberStatsCache.GetAsync(regionEnum, realm, guild);

            if (guildData == null)
            {
                return new List<GuildMemberStats>();
            }

            return guildData;
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


        [Authorize]
        [HttpPost("createGuildProfile")]
        public async Task<ActionResult> CreateGuildProfile(string name, string guild, string realm, string region)
        {
            InputValidators.ValidateProfileName(name);
            InputValidators.ValidateGuildName(guild);
            InputValidators.ValidateRealmName(realm);

            guild = BlizzardService.FormatGuildName(guild);
            realm = BlizzardService.FormatRealmName(realm);
            var regionEnum = GameRegionUtilities.GetGameRegionFromString(region);

            var locatedRealm = await this.realmStoreByValues.GetRealmAsync(realm, regionEnum);
            if (locatedRealm == null)
            {
                throw new UserReportableError($"Couldn't locate realm '{realm}'.", 404);
            }

            var locatedGuild = await this.guildCache.GetGuild(regionEnum, guild, realm);

            if (locatedGuild == null)
            {
                throw new UserReportableError("Could not locate this guild.", 404);
            }

            var user = await this.userManager.GetUserAsync(HttpContext.User);

            await this.dataRepo.CreateGuildProfileAsync(user.Id, name, locatedGuild, locatedRealm, regionEnum);

            return Ok();
        }

        [Authorize]
        [HttpPost("addMainToProfile")]
        public async Task<PlayerMain> AddMainToProfile([FromBody] AddMainToProfile input)
        {
            var user = await this.userManager.GetUserAsync(HttpContext.User);

            if (!await this.UserCanAddMainAsync(user, input.ProfileId))
            {
                throw new UserReportableError("This user doesn't have permissions to perform this operation.", 401);
            }

            InputValidators.ValidateProfileName(input.PlayerName);
            InputValidators.ValidateGuildName(input.GuildName);
            InputValidators.ValidateRealmName(input.PlayerRealmName);

            input.GuildName = BlizzardService.FormatGuildName(input.GuildName);
            input.PlayerRealmName = BlizzardService.FormatRealmName(input.PlayerRealmName);
            var regionEnum = GameRegionUtilities.GetGameRegionFromString(input.RegionName);

            var locatedPlayerRealm = await this.realmStoreByValues.GetRealmAsync(input.PlayerRealmName, regionEnum);
            if (locatedPlayerRealm == null)
            {
                throw new UserReportableError($"Couldn't locate player realm '{input.PlayerRealmName}'.", 404);
            }

            var locatedGuildRealm = await this.realmStoreByValues.GetRealmAsync(input.GuildRealmName, regionEnum);
            if (locatedGuildRealm == null)
            {
                throw new UserReportableError($"Couldn't locate guild realm '{input.GuildRealmName}'.", 404);
            }

            var locatedPlayerGuild = await this.guildStore.GetGuildAsync(input.GuildName, locatedGuildRealm, input.ProfileId);
            if (locatedPlayerGuild == null)
            {
                throw new UserReportableError($"Could not locate guild {input.GuildName}.", 404);
            }

            var player = await this.playerStore.GetPlayerAsync(input.PlayerName, locatedPlayerRealm, locatedPlayerGuild, input.ProfileId);
            if (player == null)
            {
                throw new UserReportableError($"Could not locate player {input.PlayerName}-{locatedPlayerRealm.Name}.", 404);
            }

            var newPlayer = await this.dataRepo.AddMainToProfileAsync(player.Id, input.ProfileId);

            return new PlayerMain()
            {
                Id = newPlayer.Id,
                Notes = newPlayer.Notes,
                Player = new StoredPlayer()
                {
                    Id = newPlayer.Player.Id,
                    Name = newPlayer.Player.Name,
                    Class = newPlayer.Player.Class,
                    Level = newPlayer.Player.Level
                }
            };
        }

        [Authorize]
        [HttpPost("addAltToMain")]
        public async Task<PlayerAlt> AddAltToMain([FromBody] AddAltToMain input)
        {
            var user = await this.userManager.GetUserAsync(HttpContext.User);

            if (!await this.UserCanAddAltAsync(user, input.ProfileId))
            {
                throw new UserReportableError("This user doesn't have permissions to perform this operation.", 401);
            }

            InputValidators.ValidateProfileName(input.PlayerName);
            InputValidators.ValidateGuildName(input.GuildName);
            InputValidators.ValidateRealmName(input.PlayerRealmName);

            input.GuildName = BlizzardService.FormatGuildName(input.GuildName);
            input.PlayerRealmName = BlizzardService.FormatRealmName(input.PlayerRealmName);
            var regionEnum = GameRegionUtilities.GetGameRegionFromString(input.RegionName);

            var locatedPlayerRealm = await this.realmStoreByValues.GetRealmAsync(input.PlayerRealmName, regionEnum);
            if (locatedPlayerRealm == null)
            {
                throw new UserReportableError($"Couldn't locate player realm '{input.PlayerRealmName}'.", 404);
            }

            var locatedGuildRealm = await this.realmStoreByValues.GetRealmAsync(input.GuildRealmName, regionEnum);
            if (locatedGuildRealm == null)
            {
                throw new UserReportableError($"Couldn't locate guild realm '{input.GuildRealmName}'.", 404);
            }

            var locatedPlayerGuild = await this.guildStore.GetGuildAsync(input.GuildName, locatedGuildRealm, input.ProfileId);
            if (locatedPlayerGuild == null)
            {
                throw new UserReportableError($"Could not locate guild {input.GuildName}.", 404);
            }

            var player = await this.playerStore.GetPlayerAsync(input.PlayerName, locatedPlayerRealm, locatedPlayerGuild, input.ProfileId);
            if (player == null)
            {
                throw new UserReportableError($"Could not locate player {input.PlayerName}-{locatedPlayerRealm.Name}.", 404);
            }

            var newAlt = await this.dataRepo.AddAltToMainAsync(player.Id, input.MainId, input.ProfileId);

            return new PlayerAlt()
            {
                Id = newAlt.Id,
                Player = new StoredPlayer()
                {
                    Id = newAlt.Player.Id,
                    Name = newAlt.Player.Name,
                    Class = newAlt.Player.Class,
                    Level = newAlt.Player.Level
                }
            };
        }

        [Authorize]
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

        [Authorize]
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
                }
            });

            return jsonProfiles;
        }

        [HttpGet("getGuildProfile")]
        public async Task<FullGuildProfile> GetFullGuildProfile(int profileId)
        {
            var user = await this.userManager.GetUserAsync(HttpContext.User);

            var permissionLevel = user != null
                ? await this.dataRepo.GetProfilePermissionForUserAsync(profileId, user.Id)
                : null;
            bool isOfficer = permissionLevel.HasValue
                ? PermissionsOrder.GreaterThanOrEqual(permissionLevel.Value,
                    GuildProfilePermissionLevel.Officer)
                : false;

            var repoProfile = await this.dataRepo.GetFullGuildProfileAsync(profileId);
            var efProfile = repoProfile.Profile;

            var returnProfile = new FullGuildProfile()
            {
                Id = efProfile.Id,
                ProfileName = efProfile.ProfileName,
                Creator = new UserStub()
                {
                    Id = efProfile.Creator.Id,
                    Email = efProfile.Creator.Email,
                    Username = efProfile.Creator.UserName
                },
                GuildName = efProfile.CreatorGuild.Name,
                Mains = efProfile.PlayerMains.Select(x => new PlayerMain()
                {
                    Id = x.Id,
                    Player = new StoredPlayer()
                    {
                        Id = x.Player.Id,
                        Name = x.Player.Name,
                        Class = x.Player.Class,
                        Level = x.Player.Level
                    },
                    Alts = x.Alts.Select(y => new PlayerAlt()
                    {
                        Id = y.Id,
                        Player = new StoredPlayer()
                        {
                            Id = y.Player.Id,
                            Name = y.Player.Name,
                            Class = y.Player.Class,
                            Level = y.Player.Level
                        }
                    }),
                    OfficerNotes = isOfficer ? x.OfficerNotes : string.Empty
                }),
                Realm = new StoredRealm()
                {
                    Id = efProfile.Realm.Id,
                    Name = efProfile.Realm.Name,
                    RegionId = efProfile.Realm.Region.Id
                },
                Region = efProfile.Realm.Region.RegionName,
                AccessRequestCount = isOfficer ? efProfile.AccessRequests.Count() : 0
            };

            returnProfile.Players = await this.guildMemberCache.GetMembers(
                EnumUtilities.GameRegionUtilities.GetGameRegionFromString(returnProfile.Region),
                returnProfile.Realm.Name, 
                returnProfile.GuildName);

            returnProfile.CurrentPermissionLevel = (int?)permissionLevel;

            return returnProfile;
        }

        [Authorize]
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

            Task.WaitAll(userPermissionTask, allPermissionsTask);

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

        [HttpGet("getRealms")]
        public async Task<IEnumerable<Realm>> GetRealms(string region)
        {
            return await this.realmsCache.GetRealms(EnumUtilities.GameRegionUtilities.GetGameRegionFromString(region));
        }

        private async Task<bool> UserCanAddMainAsync(EfModels.UserWithData user, int profileId)
        {
            if (await this.IsAdmin(user))
            {
                return true;
            }

            var permissionsForProfile = await this.dataRepo.GetProfilePermissionForUserAsync(profileId, user.Id);

            if (!permissionsForProfile.HasValue)
            {
                return false;
            }

            return PermissionsOrder.GetPermissionOrder(permissionsForProfile.Value)
                >= PermissionsOrder.GetPermissionOrder(GuildProfilePermissionLevel.Officer);
        }

        private async Task<bool> UserCanAddAltAsync(EfModels.UserWithData user, int profileId)
        {
            if (await this.IsAdmin(user))
            {
                return true;
            }

            var permissionsForProfile = await this.dataRepo.GetProfilePermissionForUserAsync(profileId, user.Id);

            if (!permissionsForProfile.HasValue)
            {
                return false;
            }

            return PermissionsOrder.GetPermissionOrder(permissionsForProfile.Value)
                >= PermissionsOrder.GetPermissionOrder(GuildProfilePermissionLevel.Officer);
        }

        private async Task<bool> UserCanRemoveAltAsync(EfModels.UserWithData user, int profileId)
        {
            return await this.UserCanAddAltAsync(user, profileId);
        }

        private async Task<bool> UserCanRemoveMain(EfModels.UserWithData user, int profileId)
        {
            return await this.UserCanAddAltAsync(user, profileId);
        }

        private async Task<bool> CanDeleteProfile(EfModels.UserWithData user, int profileId)
        {
            if (await this.IsAdmin(user))
            {
                return true;
            }

            var permissionsForProfile = await this.dataRepo.GetProfilePermissionForUserAsync(profileId, user.Id);

            if (!permissionsForProfile.HasValue)
            {
                return false;
            }

            return PermissionsOrder.GreaterThanOrEqual(permissionsForProfile.Value, GuildProfilePermissionLevel.Admin);
        }

        private async Task<bool> UserCanApproveAccessRequest(EfModels.UserWithData user, int profileId)
        {
            if (await this.IsAdmin(user))
            {
                return true;
            }

            var permissionsForProfile = await this.dataRepo.GetProfilePermissionForUserAsync(profileId, user.Id);

            if (!permissionsForProfile.HasValue)
            {
                return false;
            }

            return PermissionsOrder.GetPermissionOrder(permissionsForProfile.Value)
                >= PermissionsOrder.GetPermissionOrder(GuildProfilePermissionLevel.Officer);
        }

        private async Task<bool> CanGetAllProfilePermissions(EfModels.UserWithData user, int profileId)
        {
            if (await this.IsAdmin(user))
            {
                return true;
            }

            var permissionsForProfile = await this.dataRepo.GetProfilePermissionForUserAsync(profileId, user.Id);

            if (!permissionsForProfile.HasValue)
            {
                return false;
            }

            return PermissionsOrder.GetPermissionOrder(permissionsForProfile.Value)
                >= PermissionsOrder.GetPermissionOrder(GuildProfilePermissionLevel.Officer);
        }

        private async Task<bool> CanUpdatePermissions(EfModels.UserWithData user, int profileId)
        {
            return await this.UserCanApproveAccessRequest(user, profileId);
        }

        private async Task<bool> IsAdmin(EfModels.UserWithData user)
        {
            var roles = await this.GetRolesForUser(user);
            return (roles.Any(r => r == GuildToolsRoles.AdminRole.Name));
        }

        private async Task<IEnumerable<string>> GetRolesForUser(EfModels.UserWithData user)
        {
            return (await this.userManager.GetRolesAsync(user));
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
