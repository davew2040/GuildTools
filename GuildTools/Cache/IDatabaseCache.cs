using GuildTools.EF;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuildTools.Cache
{
    public interface IDatabaseCache
    {
        Task<CacheResult<T>> TryGetValueAsync<T>(string key);
        Task InsertValueAsync<T>(string key, T newValue, TimeSpan duration);
    }
}
