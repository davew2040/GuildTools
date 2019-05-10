using GuildTools.Controllers.JsonResponses;
using GuildTools.ExternalServices.Blizzard;
using GuildTools.ExternalServices.Blizzard.JsonParsing;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using static GuildTools.ExternalServices.Blizzard.BlizzardService;

namespace GuildTools.ExternalServices
{
    public class RaiderIoService : IRaiderIoService
    {
        private HttpClient client;
        private readonly ICallThrottler throttler;

        public RaiderIoService(ICallThrottler throttler, IBlizzardService blizzardService, HttpClient client)
        {
            this.client = client;
            this.throttler = throttler;
        }

        public async Task<string> GetMythicPlusDungeonData(BlizzardRegion region, string playerName, string realm)
        {
            string url = $"https://raider.io/api/v1/characters/profile?region={region.ToString().ToLower()}&"
                + $"realm={BlizzardService.FormatRealmName(realm)}&name={playerName}&fields=mythic_plus_scores";

            var result = await this.throttler.Throttle(async () => { return await client.GetAsync(url); });

            return await result.Content.ReadAsStringAsync();
        }
    }
}
