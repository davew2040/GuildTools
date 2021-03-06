﻿using GuildTools.Controllers.JsonResponses;
using GuildTools.Controllers.Models;
using GuildTools.EF.Models.Enums;
using System.Threading.Tasks;

namespace GuildTools.Cache.SpecificCaches.CacheInterfaces
{
    public interface IGuildCache
    {
        Task<GuildSlim> GetGuild(GameRegionEnum region, string guildName, string realmName);
    }
}
