using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuildTools.Controllers.Cache
{
    public interface IGuildCache
    {
        string Get(string realm, string guild);
    }
}
