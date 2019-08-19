using GuildTools.Configuration;
using GuildTools.Data;
using Microsoft.Extensions.Configuration;
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

        private const string DefaultLocale = "en_US";

        private Dictionary<BlizzardRegion, string> regionAccessTokens;
        private HttpClient client;
        private IDataRepository dataRepo;
        private readonly BlizzardApiSecrets secrets;

        public enum BlizzardRegion
        {
            US,
            EU
        }

        public BlizzardService(IDataRepository dataRepository, IConfiguration configuration, HttpClient client)
        {
            this.client = client;
            this.dataRepo = dataRepository;

            this.secrets = new BlizzardApiSecrets()
            {
                ClientId = configuration.GetValue<string>("BlizzardApiSecrets:ClientId"),
                ClientSecret = configuration.GetValue<string>("BlizzardApiSecrets:ClientSecret")
            };

            this.regionAccessTokens = new Dictionary<BlizzardRegion, string>();

            var usKeyTask = this.dataRepo.GetStoredValueAsync(this.AccessTokenCacheKeys[BlizzardRegion.US]);
            var euKeyTask = this.dataRepo.GetStoredValueAsync(this.AccessTokenCacheKeys[BlizzardRegion.EU]);

            Task.WaitAll(usKeyTask, euKeyTask);

            this.regionAccessTokens[BlizzardRegion.US] = usKeyTask.Result ?? string.Empty;
            this.regionAccessTokens[BlizzardRegion.EU] = euKeyTask.Result ?? string.Empty;
        }

        public async Task<string> GetGuildMembersAsync(string guild, string realm, BlizzardRegion region)
        {
            var regionString = GetRegionString(region);
            var ns = $"dynamic-{regionString}";
            string url = $"https://{regionString}.api.blizzard.com/wow/guild/{FormatRealmName(realm)}/{FormatGuildName(guild)}?namespace={ns}&locale={DefaultLocale}&access_token={{0}}&fields=members";

            return await this.DoBlizzardGet(url, region);
        }

        public async Task<string> GetPlayerItemsAsync(string player, string realm, BlizzardRegion region)
        {
            var regionString = GetRegionString(region);
            var ns = $"dynamic-{regionString}";
            string url = $"https://{regionString}.api.blizzard.com/wow/character/{FormatRealmName(realm)}/{player}?namespace={ns}&locale={DefaultLocale}&access_token={{0}}&fields=items";

            return await DoBlizzardGet(url, region);
        }

        public async Task<string> GetPlayerPvpStatsAsync(string player, string realm, BlizzardRegion region)
        {
            var regionString = GetRegionString(region);
            var ns = $"dynamic-{regionString}";
            string url = $"https://{GetRegionString(region)}.api.blizzard.com/wow/character/{FormatRealmName(realm)}/{player}?namespace={ns}&locale={DefaultLocale}&access_token={{0}}&fields=pvp";

            return await DoBlizzardGet(url, region);
        }

        public async Task<string> GetPlayerMountsAsync(string player, string realm, BlizzardRegion region)
        {
            var regionString = GetRegionString(region);
            var ns = $"dynamic-{regionString}";
            string url = $"https://{regionString}.api.blizzard.com/wow/character/{FormatRealmName(realm)}/{player}?namespace={ns}&locale={DefaultLocale}&access_token={{0}}&fields=mounts";

            return await DoBlizzardGet(url, region);
        }

        public async Task<string> GetPlayerPetsAsync(string player, string realm, BlizzardRegion region)
        {
            var regionString = GetRegionString(region);
            var ns = $"dynamic-{regionString}";
            string url = $"https://{regionString}.api.blizzard.com/wow/character/{FormatRealmName(realm)}/{player}?namespace={ns}&locale={DefaultLocale}&access_token={{0}}&fields=pets";

            return await DoBlizzardGet(url, region);
        }

        public async Task<string> GetGuildAsync(string guild, string realm, BlizzardRegion region)
        {
            var regionString = GetRegionString(region);
            var ns = $"dynamic-{regionString}";
            string url = $"https://{regionString}.api.blizzard.com/wow/guild/{FormatRealmName(realm)}/{FormatGuildName(guild)}?namespace={ns}&locale={DefaultLocale}&access_token={{0}}";

            return await DoBlizzardGet(url, region);
        }

        public async Task<string> GetPlayerAsync(string player, string realm, BlizzardRegion region)
        {
            var regionString = GetRegionString(region);
            var ns = $"dynamic-{regionString}";
            var url = $"https://{regionString}.api.blizzard.com/wow/character/{realm}/{player}?namespace={ns}&fields=guild&locale={DefaultLocale}&access_token={{0}}";

            return await DoBlizzardGet(url, region);
        }

        public async Task<string> GetRealmsByRegionAsync(BlizzardRegion region)
        {
            var regionString = GetRegionString(region);
            var ns = $"dynamic-{regionString}";
            string url = $"https://{regionString}.api.blizzard.com/data/wow/realm/index?namespace={ns}&locale={DefaultLocale}&access_token={{0}}";

            return await DoBlizzardGet(url, region);
        }

        public async Task<string> GetRealmAsync(string realmName, BlizzardRegion region)
        {
            var realmSlug = BuildRealmSlug(realmName);
            var regionString = GetRegionString(region);
            var ns = $"dynamic-{regionString}";

            var url = $"https://{regionString}.api.blizzard.com/data/wow/realm/{realmSlug}?namespace={ns}&locale={DefaultLocale}&access_token={{0}}";

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
            name = name.Replace('-', ' ');

            return name;
        }

        public static string BuildRealmSlug(string realmName)
        {
            realmName = realmName.Trim().ToLower();

            realmName = realmName.Replace("\'", "");
            realmName = realmName.Replace(' ', '-');

            return realmName;
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

        public static bool DidGetFail(string jsonResponse)
        {
            var jobject = JsonConvert.DeserializeObject(jsonResponse) as JObject;

            return (jobject["status"] != null && jobject["status"].ToString() == "nok");
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
                await this.dataRepo.CreateOrUpdateStoredValueAsync(this.AccessTokenCacheKeys[region], this.regionAccessTokens[region]);
            }

            client.DefaultRequestHeaders.Clear();

            HttpResponseMessage response;

            response = await client.GetAsync(string.Format(url, this.regionAccessTokens[region]));

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                this.regionAccessTokens[region] = await this.QueryAccessToken(region);
                await this.dataRepo.CreateOrUpdateStoredValueAsync(this.AccessTokenCacheKeys[region], this.regionAccessTokens[region]);

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
