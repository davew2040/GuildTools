using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AspNetAngularTemplate.Controllers.JsonResponses;
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
        private readonly IDataRepository repository;
        private readonly UserManager<IdentityUser> userManager;

        public DataController(
            IConfiguration configuration,
            IOptions<ConnectionStrings> connectionStrings,
            IDataRepository repository,
            IBlizzardService blizzardService, 
            IGuildCache guildCache, 
            IGuildMemberCache guildMemberCache,
            UserManager<IdentityUser> userManager)
        {
            this.configuration = configuration;
            this.connectionStrings = connectionStrings.Value;
            this.dataSql = new Sql.Data(this.connectionStrings.Database);
            this.blizzardService = blizzardService;
            this.guildMemberCache = guildMemberCache;
            this.repository = repository;
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
        public async Task<IActionResult> GetGuildMemberStats(string region, string guild, string realm)
        {
            guild = BlizzardService.FormatGuildName(guild);
            realm = BlizzardService.FormatRealmName(realm);
            Region regionEnum = BlizzardService.GetRegionFromString(region);

            var guildData = this.guildMemberCache.Get(regionEnum, realm, guild);

            if (guildData == null)
            {
                return new EmptyResult();
            }

            return Json(guildData);
        }

        [HttpGet("guildExists")]
        public async Task<ActionResult> GuildExists(string region, string guild, string realm)
        {
            guild = BlizzardService.FormatGuildName(guild);
            realm = BlizzardService.FormatRealmName(realm);
            
            var result = await this.blizzardService.GetGuildAsync(guild, realm, BlizzardService.GetRegionFromString(region));

            var jobject = JsonConvert.DeserializeObject(result) as JObject;
            if (jobject["status"] != null && jobject["status"].ToString() == "nok")
            {
                return Json(new GuildFound()
                {
                    Found = false
                });
            }

            return Json(new GuildFound()
            {
                Found = true,
                Realm = jobject["realm"].ToString(),
                Name = jobject["name"].ToString()
            });
        }

        [Authorize]
        [HttpPost("createGuildProfile")]
        public async Task<ActionResult> CreateGuildProfile(string guild, string realm, string region)
        {
            guild = BlizzardService.FormatGuildName(guild);
            realm = BlizzardService.FormatRealmName(realm);
            var regionEnum = BlizzardService.GetRegionFromString(region);

            var blizzardGuild = await this.blizzardService.GetGuildAsync(guild, realm, regionEnum);

            if (!this.CheckGuildOkay(blizzardGuild))
            {
                throw new ArgumentException("Could not locate this guild!");
            }

            var user = await this.userManager.GetUserAsync(HttpContext.User);

            await this.repository.CreateGuildProfileAsync(user.Id, guild, realm, BlizzardService.GetRegionFromString(region));

            return Ok();
        }

        private bool CheckGuildOkay(string jsonResponse)
        {
            var jobject = JsonConvert.DeserializeObject(jsonResponse) as JObject;

            return (jobject["status"] != null && jobject["status"].ToString() != "nok");
        }
    }
}
