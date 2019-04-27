﻿using GuildTools.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace GuildTools.ExternalServices.Blizzard
{
    public class BlizzardService : IBlizzardService
    {
        private readonly Dictionary<BlizzardRegion, string> AccessTokenCacheKeys = new Dictionary<BlizzardRegion, string>()
        {
            { BlizzardRegion.US, "UsBlizzardAccessToken" },
            { BlizzardRegion.EU, "EuBlizzardAccessToken" }
        };

        private Dictionary<BlizzardRegion, string> regionAccessTokens;
        private HttpClient client;
        private readonly Sql.Data dataSql;
        private readonly BlizzardApiSecrets secrets;

        public enum BlizzardRegion
        {
            US,
            EU
        }

        public BlizzardService(string connectionString, BlizzardApiSecrets secrets)
        {
            client = new HttpClient();

            this.secrets = secrets;
            this.dataSql = new Sql.Data(connectionString);

            this.regionAccessTokens = new Dictionary<BlizzardRegion, string>();
            this.regionAccessTokens[BlizzardRegion.US] = this.dataSql.GetStoredValue(this.AccessTokenCacheKeys[BlizzardRegion.US]) ?? string.Empty;
            this.regionAccessTokens[BlizzardRegion.EU] = this.dataSql.GetStoredValue(this.AccessTokenCacheKeys[BlizzardRegion.EU]) ?? string.Empty;
        }

        public async Task<string> GetGuildMembersAsync(string guild, string realm, BlizzardRegion region)
        {
            string url = $"https://{GetRegionString(region)}.api.blizzard.com/wow/guild/{FormatRealmName(realm)}/{FormatGuildName(guild)}?locale=en-US&access_token={{0}}&fields=members";

            return await this.DoBlizzardGet(url, region);
        }

        public async Task<string> GetPlayerItemsAsync(string player, string realm, BlizzardRegion region)
        {
            string url = $"https://{GetRegionString(region)}.api.blizzard.com/wow/character/{FormatRealmName(realm)}/{player}?locale=en-US&access_token={{0}}&fields=items";

            return await DoBlizzardGet(url, region);
        }

        public async Task<string> GetPlayerPvpStatsAsync(string player, string realm, BlizzardRegion region)
        {
            string url = $"https://{GetRegionString(region)}.api.blizzard.com/wow/character/{FormatRealmName(realm)}/{player}?locale=en-US&access_token={{0}}&fields=pvp";

            return await DoBlizzardGet(url, region);
        }

        public async Task<string> GetPlayerMountsAsync(string player, string realm, BlizzardRegion region)
        {
            string url = $"https://{GetRegionString(region)}.api.blizzard.com/wow/character/{FormatRealmName(realm)}/{player}?locale=en-US&access_token={{0}}&fields=mounts";

            return await DoBlizzardGet(url, region);
        }

        public async Task<string> GetPlayerPetsAsync(string player, string realm, BlizzardRegion region)
        {
            string url = $"https://{GetRegionString(region)}.api.blizzard.com/wow/character/{FormatRealmName(realm)}/{player}?locale=en-US&access_token={{0}}&fields=pets";

            return await DoBlizzardGet(url, region);
        }

        public async Task<string> GetGuildAsync(string guild, string realm, BlizzardRegion region)
        {   
            string url = $"https://{GetRegionString(region)}.api.blizzard.com/wow/guild/{FormatRealmName(realm)}/{FormatGuildName(guild)}?locale=en-US&access_token={{0}}&fields=pets";

            return await DoBlizzardGet(url, region);
        }

        public async Task<string> GetRealmsByRegionAsync(BlizzardRegion region)
        {
            var regionString = GetRegionString(region);
            var ns = $"dynamic-{regionString}";
            string url = $"https://{regionString}.api.blizzard.com/data/wow/realm/index?namespace={ns}&locale=en_US&access_token={{0}}";

            return await DoBlizzardGet(url, region);
        }

        public static string GetRegionString(BlizzardRegion region)
        {
            switch (region)
            {
                case BlizzardRegion.EU:
                    return "eu";
                case BlizzardRegion.US:
                default:
                    return "us";
            }
        }

        public static BlizzardRegion GetRegionFromString(string region)
        {
            region = region.Trim().ToLower();

            switch (region)
            {
                case "eu":
                    return BlizzardRegion.EU;
                case "us":
                    return BlizzardRegion.US;
                default:
                    throw new ArgumentException($"No valid region associated with region {region}.");
            }
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
            name = name.Replace("\'", "");

            return name;
        }

        public static DateTime FromUnixTime(long unixTime)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddMilliseconds(unixTime);
        }

        public static bool DidGetGuildFail(string jsonResponse)
        {
            var jobject = JsonConvert.DeserializeObject(jsonResponse) as JObject;

            return (jobject["status"] != null && jobject["status"].ToString() != "nok");
        }

        private async Task<string> QueryAccessToken(BlizzardRegion region)
        {
            string url = $"https://{GetRegionString(region)}.battle.net/oauth/token?grant_type=client_credentials";
            
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
        
        // Note: url must be pre-formatted with region!
        private async Task<string> DoBlizzardGet(string url, BlizzardRegion region)
        {
            if (string.IsNullOrEmpty(this.regionAccessTokens[region]))
            {
                this.regionAccessTokens[region] = await this.QueryAccessToken(region);
                this.dataSql.SetStoredValue(this.AccessTokenCacheKeys[region], this.regionAccessTokens[region]);
            }

            client.DefaultRequestHeaders.Clear();

            HttpResponseMessage response;

            response = await client.GetAsync(string.Format(url, this.regionAccessTokens[region]));

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                this.regionAccessTokens[region] = await this.QueryAccessToken(region);
                this.dataSql.SetStoredValue(this.AccessTokenCacheKeys[region], this.regionAccessTokens[region]);

                response = await client.GetAsync(string.Format(url, this.regionAccessTokens[region]));

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
    }
}
