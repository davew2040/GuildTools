using GuildTools.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace GuildTools.ExternalServices
{
    public class BlizzardService : IBlizzardService
    {
        private const string AccessTokenCacheKey = "BlizzardAccessToken";

        private string accessToken = string.Empty;
        private HttpClient client;
        private readonly Sql.Data dataSql;
        private readonly BlizzardApiSecrets secrets;

        public BlizzardService(string connectionString, BlizzardApiSecrets secrets)
        {
            client = new HttpClient();

            this.secrets = secrets;
            this.dataSql = new Sql.Data(connectionString);

            this.accessToken = this.dataSql.GetStoredValue(AccessTokenCacheKey) ?? string.Empty;
        }

        public async Task<string> GetGuildMembers(string guild, string realm)
        {
            string url = $"https://us.api.blizzard.com/wow/guild/{FormatRealmName(realm)}/{FormatGuildName(guild)}?locale=en-US&access_token={{0}}&fields=members";

            return await DoBlizzardGet(url);
        }

        public async Task<HttpResponseMessage> GetPlayerItemsWithResponse(string realm, string player)
        {
            string url = $"https://us.api.blizzard.com/wow/character/{FormatRealmName(realm)}/{player}?locale=en-US&access_token={{0}}&fields=items";

            return await this.DoBlizzardGetWithResponse(url);
        }

        public async Task<string> GetPlayerItems(string realm, string player)
        {
            string url = $"https://us.api.blizzard.com/wow/character/{FormatRealmName(realm)}/{player}?locale=en-US&access_token={{0}}&fields=items";

            return await DoBlizzardGet(url);
        }

        public async Task<string> GetPlayerPvpStats(string realm, string player)
        {
            string url = $"https://us.api.blizzard.com/wow/character/{FormatRealmName(realm)}/{player}?locale=en-US&access_token={{0}}&fields=pvp";

            return await DoBlizzardGet(url);
        }

        public async Task<string> GetPlayerMounts(string realm, string player)
        {
            string url = $"https://us.api.blizzard.com/wow/character/{FormatRealmName(realm)}/{player}?locale=en-US&access_token={{0}}&fields=mounts";

            return await DoBlizzardGet(url);
        }

        public async Task<string> GetPlayerPets(string realm, string player)
        {
            string url = $"https://us.api.blizzard.com/wow/character/{FormatRealmName(realm)}/{player}?locale=en-US&access_token={{0}}&fields=pets";

            return await DoBlizzardGet(url);
        }

        private async Task<string> GetAccessToken()
        {
            const string url = "https://us.battle.net/oauth/token?grant_type=client_credentials";
            
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Basic",
                Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(this.secrets.ClientId + ":" + this.secrets.ClientSecret)));

            var response = await client.GetAsync(url);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                var exception = new HttpRequestException("Error connecting to Blizzard servers.");
                exception.Data.Add("response", response);

                throw exception;
            }

            string responseJson = await response.Content.ReadAsStringAsync();
            dynamic converted = JsonConvert.DeserializeObject(responseJson);

            return converted["access_token"];
        }

        private async Task<string> DoBlizzardGet(string url)
        {
            if (string.IsNullOrEmpty(accessToken))
            {
                this.accessToken = await GetAccessToken();
                this.dataSql.SetStoredValue(AccessTokenCacheKey, this.accessToken);
            }

            client.DefaultRequestHeaders.Clear();

            HttpResponseMessage response;

            response = await client.GetAsync(string.Format(url, this.accessToken));

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                this.accessToken = await GetAccessToken();
                this.dataSql.SetStoredValue(AccessTokenCacheKey, this.accessToken);

                response = await client.GetAsync(string.Format(url, accessToken));

                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    var exception = new HttpRequestException("Error connecting to Blizzard servers.");
                    exception.Data.Add("response", response);

                    throw exception;
                }
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();

            return jsonResponse;
        }

        private async Task<HttpResponseMessage> DoBlizzardGetWithResponse(string url)
        {
            if (string.IsNullOrEmpty(accessToken))
            {
                this.accessToken = await GetAccessToken();
                this.dataSql.SetStoredValue(AccessTokenCacheKey, this.accessToken);
            }

            client.DefaultRequestHeaders.Clear();

            HttpResponseMessage response;

            response = await client.GetAsync(string.Format(url, this.accessToken));

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                this.accessToken = await GetAccessToken();
                this.dataSql.SetStoredValue(AccessTokenCacheKey, this.accessToken);

                response = await client.GetAsync(string.Format(url, accessToken));

                return response;
            }

            return response;
        }

        public static string FormatGuildName(string name)
        {
            name = name.Trim().ToLower();

            return name;
        }

        public static string FormatRealmName(string name)
        {
            name = name.Trim().ToLower();

            name = name.Replace(' ', '-');
            name = name.Replace('\'', '-');

            return name;
        }

        public static DateTime FromUnixTime(long unixTime)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddMilliseconds(unixTime);
        }
    }
}
