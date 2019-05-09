using GuildTools.EF.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static GuildTools.ExternalServices.Blizzard.BlizzardService;

namespace GuildTools.ExternalServices.Blizzard.Utilities
{
    public class BlizzardUtilities
    {
        private static Dictionary<string, int> ClassMap_StringToInt = new Dictionary<string, int>()
        {
            { "Warrior", 1 },
            { "Paladin", 2 },
            { "Hunter", 3 },
            { "Rogue", 4 },
            { "Priest", 5 },
            { "Death Knight", 6 },
            { "Mage", 8 },
            { "Warlock", 9 },
            { "Monk", 10 },
            { "Druid", 11 },
            { "Demon Hunter", 12 },
        };

        public static GameRegionEnum GetEfRegionFromBlizzardRegion(BlizzardRegion blizzardRegion)
        {
            if (blizzardRegion == BlizzardRegion.EU)
            {
                return GameRegionEnum.EU;
            }
            else if (blizzardRegion == BlizzardRegion.US)
            {
                return GameRegionEnum.US;
            }
            throw new ArgumentException("Unrecognized Blizzard region.");
        }

        public static BlizzardRegion GetBlizzardRegionFromEfRegion(GameRegionEnum efRegion)
        {
            if (efRegion == GameRegionEnum.EU)
            {
                return BlizzardRegion.EU;
            }
            else if (efRegion == GameRegionEnum.US)
            {
                return BlizzardRegion.US;
            }
            throw new ArgumentException("Unrecognized EF region.");
        }

        public static int GetBlizzardClassFromString(string className)
        {
            if (ClassMap_StringToInt.ContainsKey(className))
            {
                return ClassMap_StringToInt[className];
            }

            throw new ArgumentException("Invalid class name.");
        }
    }
}
