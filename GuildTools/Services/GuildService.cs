using GuildTools.Controllers.JsonResponses;
using GuildTools.Controllers.Models;
using GuildTools.ExternalServices;
using GuildTools.ExternalServices.Blizzard;
using GuildTools.ExternalServices.Blizzard.JsonParsing;
using GuildTools.ExternalServices.Blizzard.Utilities;
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
    public class GuildService : IGuildService
    {
        private readonly TimeSpan FilterPlayersOlderThan = new TimeSpan(90, 0, 0, 0);

        private IBlizzardService blizzardService;
        private ICallThrottler throttler;

        public GuildService(IBlizzardService blizzardService, ICallThrottler throttler)
        {
            this.blizzardService = blizzardService;
            this.throttler = throttler;
        }

        public async Task<IEnumerable<GuildMemberStats>> GetLargeGuildMembersDataAsync(BlizzardRegion region, string guild, string realm, IProgress<double> progress)
        {
            var guildDataJson = await this.blizzardService.GetGuildMembersAsync(guild, realm, region);

            var members = GuildMemberParsing.GetSlimPlayersFromGuildPlayerList(guildDataJson).ToList();

            foreach (var member in members)
            {
                member.RegionName = BlizzardService.GetRegionString(region);
                member.GuildName = guild;
            }

            int totalCount = members.Count();

            int count = 0;

            var taskList = new List<Task<GuildMemberStats>>();

            foreach (var member in members)
            {
                taskList.Add(Task.Run(async () =>
                {
                    try
                    {
                        if (await this.PopulateMemberDataAsync(member, region))
                        {
                            return member;
                        }

                        return null;
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

            return taskList.Select(x => x.Result).Where(x => x != null);
        }

        private async Task<bool> PopulateMemberDataAsync(GuildMemberStats member, BlizzardRegion region)
        {
            string items = string.Empty;
            string mounts = string.Empty;
            string pets = string.Empty;
            string pvp = string.Empty;

            var itemsTask = this.throttler.Throttle(async () =>
            {
                items = await this.blizzardService.GetPlayerItemsAsync(member.Name, member.RealmName, region);
            });

            var mountsTask = this.throttler.Throttle(async () =>
            {
                mounts = await this.blizzardService.GetPlayerMountsAsync(member.Name, member.RealmName, region);
            });

            var petsTask = this.throttler.Throttle(async () =>
            {
                pets = await this.blizzardService.GetPlayerPetsAsync(member.Name, member.RealmName, region);
            });

            var pvpTask = this.throttler.Throttle(async () =>
            {
                pvp = await this.blizzardService.GetPlayerPvpStatsAsync(member.Name, member.RealmName, region);
            });

            await Task.WhenAll(new Task[] { itemsTask, mountsTask, petsTask, pvpTask });

            try
            {
                var itemDetails = GuildMemberParsing.GetItemsDetailsFromJson(items);

                if (itemDetails.LastModifiedDateTime < DateTime.Now - FilterPlayersOlderThan)
                {
                    return false;
                }

                this.PopulateItemsDetails(member, itemDetails);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error reading player items for " + member.Name);
            }

            try
            {
                var mountsDetails = GuildMemberParsing.GetMountDetailsFromJson(mounts);

                member.MountCount = mountsDetails.NumberCollected;
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error reading mounts for " + member.Name);
            }

            try
            {
                var petDetails = GuildMemberParsing.GetPetDetailsFromJson(pets);

                member.PetCount = petDetails.NumberCollected;
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error reading pets for " + member.Name);
            }

            try
            {
                var pvpDetails = GuildMemberParsing.GetPvpStatsFromJson(pvp);

                this.PopulatePvpStats(member, pvpDetails);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error reading PvP stats for " + member.Name);
            }

            Debug.WriteLine("Processed member " + member.Name);

            return true;
        }

        private void PopulateItemsDetails(GuildMemberStats member, PlayerItemDetails itemDetails)
        {
            member.EquippedIlvl = itemDetails.EquippedIlvl;
            member.MaximumIlvl = itemDetails.MaximumIlvl;

            member.AzeriteLevel = itemDetails.AzeriteLevel.HasValue ? itemDetails.AzeriteLevel.Value : 0;
        }

        private void PopulatePvpStats(GuildMemberStats member, PvpStats pvpStats)
        {
            member.Pvp2v2Rating = pvpStats.Pvp2v2Rating;
            member.Pvp3v3Rating = pvpStats.Pvp3v3Rating;
            member.PvpRbgRating = pvpStats.PvpRbgRating;
            member.TotalHonorableKills = pvpStats.TotalHonorableKills;
        }

        public async Task<GuildSlim> GetGuild(BlizzardRegion region, string realmName, string guildName)
        {
            guildName = BlizzardService.FormatGuildName(guildName);
            realmName = BlizzardService.FormatRealmName(realmName);

            var result = await this.blizzardService.GetGuildAsync(guildName, realmName, region);

            if (BlizzardService.DidGetFail(result))
            {
                return null;
            }

            return GuildParsing.GetGuild(result);
        }

        public async Task<Realm> GetRealmAsync(string realmName, BlizzardRegion region)
        {
            var result = await this.blizzardService.GetRealmAsync(realmName, region);

            if (BlizzardService.DidGetFail(result))
            {
                return null;
            }

            return RealmParsing.GetRSingleRealm(result);
        }

        public async Task<IEnumerable<BlizzardPlayer>> GetSlimGuildMembersDataAsync(BlizzardRegion region, string guild, string realm)
        {
            var guildDataJson = await this.blizzardService.GetGuildMembersAsync(guild, realm, region);

            var members = GuildMemberParsing.GetSlimPlayersFromGuildPlayerList(guildDataJson).ToList();

            return members.Select(x => new BlizzardPlayer()
            {
                GuildName = guild,
                PlayerName = x.Name,
                PlayerRealmName = x.RealmName,
                Class = x.Class,
                Level = x.Level
            });
        }

        public async Task<BlizzardPlayer> GetSinglePlayerAsync(BlizzardRegion region, string realmName, string playerName)
        {
            var playerJson = await this.blizzardService.GetPlayerAsync(playerName, realmName, region);

            if (BlizzardService.DidGetFail(playerJson))
            {
                return null;
            }

            var player = PlayerParsing.GetSinglePlayerFromJson(playerJson);

            return new BlizzardPlayer()
            {
                PlayerName = player.Name,
                Class = player.Class,
                Level = player.Level,
                GuildName = player.GuildName,
                GuildRealm = player.GuildRealm,
                PlayerRealmName = player.Realm
            };
        }

        private string GetRegionString(BlizzardRegion region)
        {
            return region.ToString().ToUpper();
        }
    }
}
