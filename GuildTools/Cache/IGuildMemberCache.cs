﻿using GuildTools.Controllers.JsonResponses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static GuildTools.ExternalServices.Blizzard.BlizzardService;

namespace GuildTools.Cache
{
    public interface IGuildMemberCache
    {
        IEnumerable<GuildMember> Get(Region region, string realm, string guild);
        Task Refresh(Region reigon, string guild, string realm);
    }
}
