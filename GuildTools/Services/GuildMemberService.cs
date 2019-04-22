using GuildTools.Controllers.JsonResponses;
using GuildTools.ExternalServices;
using GuildTools.JsonParsing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using static GuildTools.ExternalServices.BlizzardService;
using static GuildTools.JsonParsing.GuildMemberParsing;

namespace GuildTools.Services
{
    public class GuildMemberService : IGuildMemberService
    {
        private readonly TimeSpan FilterPlayersOlderThan = new TimeSpan(90, 0, 0, 0);

        private IBlizzardService blizzardService;

        public GuildMemberService(IBlizzardService blizzardService)
        {
            this.blizzardService = blizzardService;
        }

        public async Task<IEnumerable<GuildMember>> GetGuildMemberDataAsync(Region region, string guild, string realm)
        {
            var guildDataJson = await this.blizzardService.GetGuildMembers(guild, realm, region);

            var members = GuildMemberParsing.GetSlimPlayersFromGuildPlayerList(guildDataJson).ToList();

            List<GuildMember> validMembers = new List<GuildMember>();

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

        private async Task<bool> PopulateMemberDataAsync(GuildMember member, Region region)
        {
            var itemsTask = this.blizzardService.GetPlayerItems(member.Name, member.Realm, region);
            var mountsTask = this.blizzardService.GetPlayerMounts(member.Name, member.Realm, region);
            var petsTask = this.blizzardService.GetPlayerPets(member.Name, member.Realm, region);
            var pvpTask = this.blizzardService.GetPlayerPvpStats(member.Name, member.Realm, region);

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

        private void PopulateItemsDetails(GuildMember member, PlayerItemDetails itemDetails)
        {
            member.EquippedIlvl = itemDetails.EquippedIlvl;
            member.MaximumIlvl = itemDetails.MaximumIlvl;

            member.AzeriteLevel = itemDetails.AzeriteLevel.HasValue ? itemDetails.AzeriteLevel.Value : 0;
        }

        private void PopulatePvpStats(GuildMember member, PvpStats pvpStats)
        {
            member.Pvp2v2Rating = pvpStats.Pvp2v2Rating;
            member.Pvp3v3Rating = pvpStats.Pvp3v3Rating;
            member.PvpRbgRating = pvpStats.PvpRbgRating;
            member.TotalHonorableKills = pvpStats.TotalHonorableKills;
        }

    }
}
