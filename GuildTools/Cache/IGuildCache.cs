using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static GuildTools.ExternalServices.BlizzardService;

namespace GuildTools.Cache
{
    public interface IGuildCache
    {
        string Get(Region region, string realm, string guild);
    }
}
