using GuildTools.Controllers.JsonResponses;
using GuildTools.Controllers.Models;
using GuildTools.ExternalServices;
using GuildTools.ExternalServices.Blizzard;
using GuildTools.ExternalServices.Blizzard.JsonParsing;
using GuildTools.ExternalServices.Raiderio.JsonParsing;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using static GuildTools.ExternalServices.Blizzard.BlizzardService;
using static GuildTools.ExternalServices.Blizzard.JsonParsing.GuildMemberParsing;

namespace GuildTools.Services
{
    public class LocalRaiderIoService : ILocalRaiderIoService
    {
        private readonly TimeSpan FilterPlayersOlderThan = new TimeSpan(90, 0, 0, 0);
        private readonly int FilterPlayersUnderLevel = 120;

        private IBlizzardService blizzardService;
        private IRaiderIoService raiderIoService;

        public LocalRaiderIoService(IBlizzardService blizzardService, IRaiderIoService raiderIoService)
        {
            this.blizzardService = blizzardService;
            this.raiderIoService = raiderIoService;
        }

        public async Task<IEnumerable<RaiderIoStats>> GetGuildRaiderIoStats(BlizzardRegion region, string guild, string realm, IProgress<double> progress)
        {
            var guildDataJson = await this.blizzardService.GetGuildMembersAsync(guild, realm, region);

            var members = GuildMemberParsing.GetSlimPlayersFromGuildPlayerList(guildDataJson).ToList();

            int totalCount = members.Count();
            int count = 0;

            var taskList = new List<Task<RaiderIoStats>>();

            foreach (var member in members)
            {
                taskList.Add(Task.Run(async () =>
                {
                    try
                    {
                        var result = await this.GetRaiderIoStatsAsync(member, region);

                        if (result != null)
                        {
                            result.RegionName = BlizzardService.GetRegionString(region);
                            result.GuildName = guild;
                        }

                        return result;
                    }
                    catch (Exception e)
                    {
                        return null;
                    }
                    finally
                    {
                        progress.Report((double)count++ / totalCount);
                    }
                }));
            }

            await Task.WhenAll(taskList);

            var allRaiderIoStats = taskList.Select(x => x.Result).Where(x => x != null);

            return allRaiderIoStats;
        }

        private async Task<RaiderIoStats> GetRaiderIoStatsAsync(GuildMemberStats member, BlizzardRegion region)
        {
            var raiderIoJson = await this.raiderIoService.GetMythicPlusDungeonData(region, member.Name, member.RealmName);

            if (!RaiderIoParsing.GetRequestSucceeded(raiderIoJson))
            {
                return null;
            }

            var playerDetails = RaiderIoParsing.GetPlayerFromJson(raiderIoJson);

            Debug.WriteLine("Processed Raider.IO player " + member.Name);

            return playerDetails;
        }
    }
}
