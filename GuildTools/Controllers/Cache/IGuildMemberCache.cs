using GuildTools.Controllers.JsonResponses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuildTools.Controllers.Cache
{
    public interface IGuildMemberCache
    {
        IEnumerable<GuildMember> Get(string realm, string guild);
        Task Refresh(string guild, string realm);
    }
}
