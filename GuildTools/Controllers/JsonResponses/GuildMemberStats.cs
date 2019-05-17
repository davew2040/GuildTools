using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuildTools.Controllers.JsonResponses
{
    public class GuildMemberStats
    {
        public string Name { get; set; }
        public string GuildName { get; set; }
        public string RealmName { get; set; }
        public string RegionName { get; set; }
        public int Level { get; set; }
        public int EquippedIlvl { get; set; }
        public int Class { get; set; }
        public int MaximumIlvl { get; set; }
        public int AchievementPoints { get; set; }
        public int MountCount { get; set; }
        public int PetCount { get; set; }
        public int Pvp2v2Rating { get; set; }
        public int Pvp3v3Rating { get; set; }
        public int PvpRbgRating { get; set; }
        public int TotalHonorableKills { get; set; }
        public int GuildRank { get; set; }
        public int AzeriteLevel { get; set; }
    }
}
