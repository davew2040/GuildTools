using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuildTools.Cache
{
    public abstract class Cache<T>
    {
        public abstract Task<T> TryGetValueAsync(string key);
        public abstract Task InsertValueAsync(string key, T newValue);
    }
}
