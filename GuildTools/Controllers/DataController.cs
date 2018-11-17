using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AspNetAngularTemplate.Controllers.JsonResponses;
using GuildTools.Controllers.Cache;
using GuildTools.ExternalServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GuildTools.Controllers
{
    [Route("api/[controller]")]
    public class DataController : Controller
    {
        private readonly Sql.Data dataSql;
        private readonly ConnectionStrings connectionStrings;
        private readonly IBlizzardService blizzardService;
        private readonly IGuildCache guildCache;
        private readonly IGuildMemberCache guildMemberCache;

        public DataController(
            IConfiguration configuration, 
            IOptions<ConnectionStrings> connectionStrings, 
            IBlizzardService blizzardService, 
            IGuildCache guildCache, 
            IGuildMemberCache guildMemberCache)
        {
            this.connectionStrings = connectionStrings.Value;
            this.dataSql = new Sql.Data(this.connectionStrings.Database);
            this.blizzardService = blizzardService;
            this.guildCache = guildCache;
            this.guildMemberCache = guildMemberCache;
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

        [HttpGet]
        [Route("private")]
        [Authorize]
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
                    Realm = profile.Realm
                });
            }

            return new EmptyResult();
        }

        [HttpGet("getGuildMemberStats")]
        public async Task<IActionResult> GetGuildMemberStats(string guild, string realm)
        {
            guild = BlizzardService.FormatGuildName(guild);
            realm = BlizzardService.FormatRealmName(realm);

            var guildData = this.guildMemberCache.Get(realm, guild);

            if (guildData == null)
            {
                return new EmptyResult();
            }

            return Json(guildData);
        }

        [HttpGet("guildExists")]
        public async Task<ActionResult> GuildExists(string guild, string realm)
        {
            guild = BlizzardService.FormatGuildName(guild);
            realm = BlizzardService.FormatRealmName(realm);
            
            try
            {
                var result = await this.blizzardService.GetGuildMembers(guild, realm);

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
            catch (Exception e)
            {
                return Json(new GuildFound()
                {
                    Found = false
                });
            }
        }


        [HttpGet("database")]
        public ActionResult<IEnumerable<string>> Get()
        {
            using (SqlConnection connection = new SqlConnection())
            using (SqlCommand command = new SqlCommand())
            {
                connection.ConnectionString = this.connectionStrings.Database;

                command.Connection = connection;
                command.CommandType = System.Data.CommandType.Text;
                command.CommandText = "SELECT * FROM Numbers";

                connection.Open();

                SqlDataReader reader = command.ExecuteReader();

                while (reader.HasRows)
                {
                    Debug.WriteLine("\t{0}\t{1}", reader.GetName(0),
                        reader.GetName(1));

                    while (reader.Read())
                    {
                        Debug.WriteLine("\t{0}\t{1}", reader.GetString(0),
                            reader.GetString(1));
                    }

                    reader.NextResult();
                }
            }

            return new string[] { "value1", "value2" };
        }
    }
}
