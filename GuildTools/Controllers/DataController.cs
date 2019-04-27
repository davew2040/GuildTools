using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using GuildTools.Controllers.JsonResponses;
using GuildTools.Cache;
using GuildTools.Data;
using GuildTools.ExternalServices;
using GuildTools.ExternalServices.Blizzard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static GuildTools.ExternalServices.Blizzard.BlizzardService;
using GuildTools.Controllers.Models;
using GuildTools.Cache.SpecificCaches;
using GuildTools.ExternalServices.Blizzard.Utilities;
using GuildTools.EF.Models.Enums;

namespace GuildTools.Controllers
{
    [Route("api/[controller]")]
    public class DataController : Controller
    {
        private readonly Sql.Data dataSql;
        private readonly IConfiguration configuration;
        private readonly ConnectionStrings connectionStrings;
        private readonly IBlizzardService blizzardService;
        private readonly IGuildMemberCache guildMemberCache;
        private readonly IDataRepository dataRepo;
        private readonly RealmsCache realmsCache;
        private readonly UserManager<IdentityUser> userManager;

        public DataController(
            IConfiguration configuration,
            IOptions<ConnectionStrings> connectionStrings,
            IDataRepository repository,
            IBlizzardService blizzardService, 
            IGuildMemberCache guildMemberCache,
            RealmsCache realmsCache,
            UserManager<IdentityUser> userManager)
        {
            this.configuration = configuration;
            this.connectionStrings = connectionStrings.Value;
            this.dataSql = new Sql.Data(this.connectionStrings.Database);
            this.blizzardService = blizzardService;
            this.guildMemberCache = guildMemberCache;
            this.dataRepo = repository;
            this.userManager = userManager;
            this.realmsCache = realmsCache;
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

        [HttpGet("guildProfile")]
        public ActionResult<string> GuildProfile(int id)
        {
            var profile = this.dataSql.GetGuildProfile(id);

            if (profile != null)
            {
                return Json(new
                {
                    GuildName = profile.Name,
                    profile.Realm
                });
            }

            return new EmptyResult();
        }

        [HttpGet("getGuildMemberStats")]
        public async Task<IEnumerable<GuildMember>> GetGuildMemberStats(string region, string guild, string realm)
        {
            guild = BlizzardService.FormatGuildName(guild);
            realm = BlizzardService.FormatRealmName(realm);
            BlizzardRegion regionEnum = BlizzardService.GetRegionFromString(region);

            var guildData = await this.guildMemberCache.GetAsync(regionEnum, realm, guild);

            if (guildData == null)
            {
                return new List<GuildMember>();
            }

            return guildData;
        }

        [HttpGet("guildExists")]
        public async Task<GuildFound> GuildExists(string region, string guild, string realm)
        {
            guild = BlizzardService.FormatGuildName(guild);
            realm = BlizzardService.FormatRealmName(realm);
            
            var result = await this.blizzardService.GetGuildAsync(guild, realm, BlizzardService.GetRegionFromString(region));

            var jobject = JsonConvert.DeserializeObject(result) as JObject;
            if (jobject["status"] != null && jobject["status"].ToString() == "nok")
            {
                return new GuildFound()
                {
                    Found = false
                };
            }

            return new GuildFound()
            {
                Found = true,
                RealmName = jobject["realm"].ToString(),
                GuildName = jobject["name"].ToString()
            };
        }

        [Authorize]
        [HttpPost("createGuildProfile")]
        public async Task<ActionResult> CreateGuildProfile(string name, string guild, string realm, string region)
        {
            guild = BlizzardService.FormatGuildName(guild);
            realm = BlizzardService.FormatRealmName(realm);
            var regionEnum = BlizzardService.GetRegionFromString(region);

            var blizzardGuild = await this.blizzardService.GetGuildAsync(guild, realm, regionEnum);

            if (BlizzardService.DidGetGuildFail(blizzardGuild))
            {
                throw new ArgumentException("Could not locate this guild!");
            }

            var user = await this.userManager.GetUserAsync(HttpContext.User);

            await this.dataRepo.CreateGuildProfileAsync(user.Id, name, guild, realm, BlizzardService.GetRegionFromString(region));

            return Ok();
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
            //var profiles = await this.dataRepo.GetGuildProfilesForUserAsync(user.Id);

            var jsonProfiles = profiles.Select(p => new GuildProfile()
            {
                Id = p.Id,
                ProfileName = p.ProfileName,
                GuildName = p.GuildName,
                Realm = p.Realm,
                Region = p.Region.RegionName,
                Creator = new CreatorStub()
                {
                    Id = p.Creator.UserId,
                    Email = user.Email,
                    Username = p.Creator.Username
                }
            });

            return jsonProfiles;
        }

        [HttpGet("getRealms")]
        public async Task<IEnumerable<Realm>> GetRealms(string region)
        {
            return await this.realmsCache.GetRealms(EnumUtilities.GameRegionUtilities.GetGameRegionFromString(region));
        }
    }
}
