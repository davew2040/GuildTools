using GuildTools.Cache;
using GuildTools.Cache.LongRunningRetrievers.Interfaces;
using GuildTools.Controllers.JsonResponses;
using GuildTools.ExternalServices.Blizzard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuildTools.Mocks
{
    public class MockRaiderIoStatsRetriever : IRaiderIoStatsRetriever
    {
        public async Task<CacheEntry<IEnumerable<RaiderIoStats>>> GetCachedEntry(BlizzardService.BlizzardRegion region, string realm, string guild)
        {
            List<RaiderIoStats> statsList = new List<RaiderIoStats>();

            statsList.Add(new RaiderIoStats()
            {
                Name = "Kromthael",
                Class = 6,
                RealmName = "Burning Blade",
                RaiderIoDps = 500,
                RaiderIoTank = 1000,
                RaiderIoHealer = 2000,
                RaiderIoOverall = 3000,
            });

            statsList.Add(new RaiderIoStats()
            {
                Name = "Kromp",
                Class = 4,
                RealmName = "Burning Blade",
                RaiderIoDps = 2000,
                RaiderIoTank = 1500,
                RaiderIoHealer = 0,
                RaiderIoOverall = 1500,
            });

            statsList.Add(new RaiderIoStats()
            {
                Name = "Kromzul",
                Class = 2,
                RealmName = "Burning Blade",
                RaiderIoDps = 1500,
                RaiderIoTank = 2000,
                RaiderIoHealer = 0,
                RaiderIoOverall = 1000,
            });

            return new CacheEntry<IEnumerable<RaiderIoStats>>()
            {
                CompletionProgress = 1.0,
                CreatedOn = DateTime.Now,
                State = CachedValueState.FoundAndNotUpdating,
                Value = statsList
            };
        }

        public string GetKey(BlizzardService.BlizzardRegion region, string realm, string guild)
        {
            throw new NotImplementedException();
        }

        public int? GetPositionInQueue(string key)
        {
            return null;
        }
    }
}
