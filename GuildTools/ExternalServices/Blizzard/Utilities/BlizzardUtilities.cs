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
        public static GameRegion GetEfRegionFromBlizzardRegion(BlizzardRegion blizzardRegion)
        {
            if (blizzardRegion == BlizzardRegion.EU)
            {
                return GameRegion.EU;
            }
            else if (blizzardRegion == BlizzardRegion.US)
            {
                return GameRegion.US;
            }
            throw new ArgumentException("Unrecognized Blizzard region.");
        }

        public static BlizzardRegion GetBlizzardRegionFromEfRegion(GameRegion efRegion)
        {
            if (efRegion == GameRegion.EU)
            {
                return BlizzardRegion.EU;
            }
            else if (efRegion == GameRegion.US)
            {
                return BlizzardRegion.US;
            }
            throw new ArgumentException("Unrecognized EF region.");
        }
    }
}
