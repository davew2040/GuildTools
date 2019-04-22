using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GuildTools.Cache
{
    public interface IKeyedResourceManager
    {
        Task<SemaphoreSlim> AcquireKeyLockAsync(string key);
        Task ReleaseKeyLockAsync(string key);
    }
}
