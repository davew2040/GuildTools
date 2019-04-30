using GuildTools.Controllers.JsonResponses;
using GuildTools.Controllers.Models;
using GuildTools.ExternalServices;
using GuildTools.ExternalServices.Blizzard;
using GuildTools.ExternalServices.Blizzard.JsonParsing;
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

        public GuildService(IBlizzardService blizzardService)
        {
            this.blizzardService = blizzardService;
        }

        public async Task<IEnumerable<GuildMemberStats>> GetLargeGuildMembersDataAsync(BlizzardRegion region, string guild, string realm)
        {
            var guildDataJson = await this.blizzardService.GetGuildMembersAsync(guild, realm, region);

            var members = GuildMemberParsing.GetSlimPlayersFromGuildPlayerList(guildDataJson).ToList();

            List<GuildMemberStats> validMembers = new List<GuildMemberStats>();

            foreach (var member in members)
            {
                try
                {
                    if (await this.PopulateMemberDataAsync(member, region))
                    {
                        validMembers.Add(member);
                    }
                }
                catch
                {
                    //Swallow any errors
                }
            }

            return validMembers;
        }

        private async Task<bool> PopulateMemberDataAsync(GuildMemberStats member, BlizzardRegion region)
        {
            var itemsTask = this.blizzardService.GetPlayerItemsAsync(member.Name, member.Realm, region);
            var mountsTask = this.blizzardService.GetPlayerMountsAsync(member.Name, member.Realm, region);
            var petsTask = this.blizzardService.GetPlayerPetsAsync(member.Name, member.Realm, region);
            var pvpTask = this.blizzardService.GetPlayerPvpStatsAsync(member.Name, member.Realm, region);

            await Task.WhenAll(new Task[] { itemsTask, mountsTask, petsTask, pvpTask });

            try
            {
                var itemDetails = GuildMemberParsing.GetItemsDetailsFromJson(itemsTask.Result);

                if (itemDetails.LastModifiedDateTime < DateTime.Now - FilterPlayersOlderThan)
                {
                    return false;
                }

                this.PopulateItemsDetails(member, itemDetails);
            }
            catch { Debug.WriteLine("Error reading player items for " + member.Name); }

            try
            {
                var mountsDetails = GuildMemberParsing.GetMountDetailsFromJson(mountsTask.Result);

                member.MountCount = mountsDetails.NumberCollected;
            }
            catch { Debug.WriteLine("Error reading mounts for " + member.Name); }

            try
            {
                var petDetails = GuildMemberParsing.GetPetDetailsFromJson(petsTask.Result);

                member.PetCount = petDetails.NumberCollected;
            }
            catch { Debug.WriteLine("Error reading pets for " + member.Name); }

            try
            {
                var pvpDetails = GuildMemberParsing.GetPvpStatsFromJson(pvpTask.Result);

                this.PopulatePvpStats(member, pvpDetails);
            }
            catch { Debug.WriteLine("Error reading PvP stats for " + member.Name); }

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

        public async Task<GuildSlim> GetGuild(BlizzardRegion region, string guild, string realm)
        {
            guild = BlizzardService.FormatGuildName(guild);
            realm = BlizzardService.FormatRealmName(realm);

            var result = await this.blizzardService.GetGuildAsync(guild, realm, region);

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
                RealmName = x.Realm,
                Class = x.Class,
                Level = x.Level
            });
        }

        public async Task<BlizzardPlayer> GetSingleGuildMemberAsync(BlizzardRegion region, string realmName, string playerName)
        {
            var playerJson = await this.blizzardService.GetPlayerAsync(playerName, realmName, region);

            var player = PlayerParsing.GetSinglePlayerFromJson(playerJson);

            return new BlizzardPlayer()
            {
                PlayerName = player.Name,
                Class = player.Class,
                Level = player.Level
            };
        }
    }
}
