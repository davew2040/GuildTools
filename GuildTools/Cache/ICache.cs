using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuildTools.Cache
{
    public interface ICache
    {
        Task<CacheResult<T>> TryGetValueAsync<T>(string key);
        Task InsertValueAsync<T>(string key, T newValue, TimeSpan expiresAfter);
        Task RemoveAsync(string key);
    }
}
