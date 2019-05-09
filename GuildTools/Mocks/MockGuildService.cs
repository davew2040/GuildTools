using GuildTools.Controllers.JsonResponses;
using GuildTools.Controllers.Models;
using GuildTools.ExternalServices.Blizzard;
using GuildTools.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace GuildTools.Mocks
{
    public class MockGuildService : IGuildService
    {
        public async Task<GuildSlim> GetGuild(BlizzardService.BlizzardRegion region, string realmName, string playerName)
        {
            return new GuildSlim()
            {
                Battlegroup = "Vindication",
                Name = "Longanimity",
                Realm = "Burning Blade"
            };
        }

        public async Task<IEnumerable<GuildMemberStats>> GetLargeGuildMembersDataAsync(BlizzardService.BlizzardRegion region, string guild, string realm, IProgress<double> progress)
        {
            int numRepeats = 15;
            double numSeconds = 60.0;

            for (int i = 0; i < numRepeats; i++)
            {
                Debug.WriteLine((double)i / numRepeats);
                progress.Report((double)i / numRepeats);
                await Task.Delay((int)(numSeconds*1000 / numRepeats));
            }

            return new List<GuildMemberStats>()
            {
                new GuildMemberStats()
                {
                    Name = "Kromp",
                    AchievementPoints = 10000,
                    AzeriteLevel = 50,
                    EquippedIlvl = 400,
                    Class = 4,
                    GuildRank = 9,
                    Level = 120,
                    MaximumIlvl = 415,
                    MountCount = 400,
                    PetCount = 500,
                    Pvp2v2Rating = 1000,
                    Pvp3v3Rating = 1500,
                    PvpRbgRating = 2000,
                    Realm = "Burning Blade",
                    TotalHonorableKills = 25000
                }
            };
        }

        public Task<Realm> GetRealmAsync(string realmName, BlizzardService.BlizzardRegion region)
        {
            throw new NotImplementedException();
        }

        public Task<BlizzardPlayer> GetSinglePlayerAsync(BlizzardService.BlizzardRegion region, string realmName, string playerName)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<BlizzardPlayer>> GetSlimGuildMembersDataAsync(BlizzardService.BlizzardRegion region, string guild, string realm)
        {
            throw new NotImplementedException();
        }
    }
}
