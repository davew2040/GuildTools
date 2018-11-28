using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace GuildTools.ExternalServices
{
    public class RaiderIoService : IRaiderIoService
    {
        private HttpClient client;

        public RaiderIoService()
        {
            client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<string> GetMythicPlusDungeonData(string playerName, string realm)
        {
            string url = $"https://raider.io/api/v1/characters/profile?region=us&realm={BlizzardService.FormatRealmName(realm)}&name={playerName}&fields=mythic_plus_scores";

            var result = await client.GetAsync(url);

            return await result.Content.ReadAsStringAsync();
        }
    }
}
