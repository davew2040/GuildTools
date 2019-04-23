using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static GuildTools.ExternalServices.Blizzard.BlizzardService;

namespace GuildTools.ExternalServices.Blizzard.Utilities
{
    public class Utilities
    {
        public static EF.Models.Enums.GameRegion GetEfRegionFromBlizzardRegion(Region blizzardRegion)
        {
            if (blizzardRegion == Region.EU)
            {
                return EF.Models.Enums.GameRegion.EU;
            }
            else if (blizzardRegion == Region.US)
            {
                return EF.Models.Enums.GameRegion.US;
            }
            throw new ArgumentException("Unrecognized Blizzard region.");
        }
    }
}
