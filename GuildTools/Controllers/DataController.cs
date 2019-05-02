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
        private readonly IGuildMemberCache guildMemberCache;
        private readonly IGuildService guildService;
        private readonly IGuildStoreByName guildStore;
        private readonly IPlayerStoreByValue playerStore;
        private readonly IRealmStoreByValues realmStoreByValues;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UserManager<IdentityUser> userManager;

        public DataController(
            IOptions<ConnectionStrings> connectionStrings,
            IDataRepository repository,
            IGuildService guildService,
            IBlizzardService blizzardService,
            IOldGuildMemberCache oldGuildMemberCache,
            IGuildCache guildCache,
            IGuildMemberCache guildMemberCache,
            IRealmsCache realmsCache,
            IGuildStoreByName guildStore,
            IPlayerStoreByValue playerStore,
            IRealmStoreByValues realmStoreByValues,
            RoleManager<IdentityRole> roleManager,
            UserManager<IdentityUser> userManager)
        {
            this.connectionStrings = connectionStrings.Value;
            this.blizzardService = blizzardService;
            this.guildMemberStatsCache = oldGuildMemberCache;
            this.guildService = guildService;
            this.dataRepo = repository;
            this.guildCache = guildCache;
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
                Creator = new CreatorStub()
                {
                    Id = p.Creator.UserId,
                    Email = user.Email,
                    Username = p.Creator.Username
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

            var repoProfile = await this.dataRepo.GetFullGuildProfile(profileId);
            var efProfile = repoProfile.Profile;

            var creatorUser = await this.userManager.FindByIdAsync(efProfile.Creator.UserId);

            var returnProfile = new FullGuildProfile()
            {
                Id = efProfile.Id,
                ProfileName = efProfile.ProfileName,
                Creator = new CreatorStub()
                {
                    Id = creatorUser.Id,
                    Email = creatorUser.Email,
                    Username = creatorUser.UserName
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
                Region = efProfile.Realm.Region.RegionName
            };

            returnProfile.Players = await this.guildMemberCache.GetMembers(
                EnumUtilities.GameRegionUtilities.GetGameRegionFromString(returnProfile.Region),
                returnProfile.Realm.Name, 
                returnProfile.GuildName);

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

            await this.dataRepo.DeleteProfile(profileId);
        }

        [HttpGet("getRealms")]
        public async Task<IEnumerable<Realm>> GetRealms(string region)
        {
            return await this.realmsCache.GetRealms(EnumUtilities.GameRegionUtilities.GetGameRegionFromString(region));
        }

        private async Task<bool> UserCanAddMainAsync(IdentityUser user, int profileId)
        {
            var roles = await this.GetRolesForUser(user);
            if (roles.Any(r => r == GuildToolsRoles.AdminRole.Name))
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

        private async Task<bool> UserCanAddAltAsync(IdentityUser user, int profileId)
        {
            var roles = await this.GetRolesForUser(user);
            if (roles.Any(r => r == GuildToolsRoles.AdminRole.Name))
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

        private async Task<bool> UserCanRemoveAltAsync(IdentityUser user, int profileId)
        {
            return await this.UserCanAddAltAsync(user, profileId);
        }

        private async Task<bool> UserCanRemoveMain(IdentityUser user, int profileId)
        {
            return await this.UserCanAddAltAsync(user, profileId);
        }

        private async Task<bool> CanDeleteProfile(IdentityUser user, int profileId)
        {
            var roles = await this.GetRolesForUser(user);
            if (roles.Any(r => r == GuildToolsRoles.AdminRole.Name))
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

        private async Task<IEnumerable<string>> GetRolesForUser(IdentityUser user)
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
