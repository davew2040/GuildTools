﻿using GuildTools.EF;
using GuildTools.EF.Models;
using GuildTools.EF.Models.Enums;
using GuildTools.EF.Models.StoredBlizzardModels;
using GuildTools.ExternalServices.Blizzard;
using GuildTools.ExternalServices.Blizzard.Utilities;
using GuildTools.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EfEnums = GuildTools.EF.Models.Enums;

namespace GuildTools.Cache.SpecificCaches.CacheInterfaces
{
    public interface IRealmStoreByValues
    {
        Task<StoredRealm> GetRealmAsync(string realmName, EfEnums.GameRegionEnum region);
    }
}
