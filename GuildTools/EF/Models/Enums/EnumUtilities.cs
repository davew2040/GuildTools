using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuildTools.EF.Models.Enums
{
    public class EnumUtilities
    {
        public static IEnumerable<T> GetEnumValues<T>() where T : Enum
        {
            var values = Enum.GetValues(typeof(T)).Cast<T>();

            return values;
        }

        public static class GameRegionUtilities
        {
            public static GameRegionEnum GetGameRegionFromString(string region)
            {
                var upper = region.ToUpper();

                switch (upper)
                {
                    case "US":
                        return GameRegionEnum.US;
                    case "EU":
                        return GameRegionEnum.EU;
                    default:
                        throw new ArgumentException("Unrecognized region type!");
                }
            }
        }
    }
}
