using GuildTools.Cache;
using GuildTools.Cache.LongRunningRetrievers.Interfaces;
using GuildTools.Controllers.JsonResponses;
using GuildTools.ExternalServices.Blizzard;
using GuildTools.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace GuildTools.Mocks
{
    public class MockLocalRaiderIoService : ILocalRaiderIoService
    {
        public async Task<IEnumerable<RaiderIoStats>> GetGuildRaiderIoStats(BlizzardService.BlizzardRegion region, string guild, string realm, IProgress<double> progress)
        {
            int numRepeats = 15;
            double numSeconds = 60.0;

            for (int i = 0; i < numRepeats; i++)
            {
                Debug.WriteLine((double)i / numRepeats);
                progress.Report((double)i / numRepeats);
                await Task.Delay((int)(numSeconds * 1000 / numRepeats));
            }

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

            return statsList;
        }
    }
}
