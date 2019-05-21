using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GuildTools.Cache
{
    public enum CachedValueState
    {
        Updating,
        FoundButUpdating,
        FoundAndNotUpdating
    }

    public class CacheEntry<T>
    {
        public CachedValueState State { get; set; }
        public T Value { get; set; }
        public DateTime CreatedOn { get; set; }
        public double CompletionProgress { get; set; }
    }

    public class LongRunningCache<T>
    {
        private ICache updatingCache;
        private IDatabaseCache longTermCache;
        private TimeSpan expiresAfter;
        private TimeSpan? refreshAfter;
        private SemaphoreSlim semaphore = new SemaphoreSlim(1);

        public LongRunningCache(ICache updatingCache, IDatabaseCache longTermCache, TimeSpan expiresAfter, TimeSpan? refreshAfter = null)
        {
            this.updatingCache = updatingCache;
            this.longTermCache = longTermCache;

            if (expiresAfter < refreshAfter)
            {
                throw new ArgumentException("Refreshes must occur before expiration!");
            }

            this.expiresAfter = expiresAfter;
            this.refreshAfter = refreshAfter;
        }

        public async Task<CacheEntry<T>> GetOrRefreshCachedValueAsync(
            string key, 
            Func<Func<CancellationToken, IServiceProvider, Task<T>>, Task> taskRunner, 
            Func<IServiceProvider, IProgress<double>, Task<T>> valueRetriever)
        {
            await semaphore.WaitAsync();

            try
            {
                CacheEntry<T> cachedValue = null;

                var cacheResult = await this.updatingCache.TryGetValueAsync<CacheEntry<T>>(key);

                if (cacheResult.Found)
                {
                    cachedValue = cacheResult.Result;

                    if (this.refreshAfter.HasValue)
                    {
                        if (DateTime.Now > cachedValue.CreatedOn + this.refreshAfter.Value
                            && cachedValue.State == CachedValueState.FoundAndNotUpdating)
                        {
                            await taskRunner(async (token, serviceProvider) =>
                            {
                                using (var serviceScope = serviceProvider.CreateScope())
                                {
                                    var cache = serviceScope.ServiceProvider.GetService<ICache>();

                                    var result = await valueRetriever(serviceProvider, new Progress<double>());
                                    await this.InsertData(cache, key, result);
                                    return result;
                                }
                            });

                            var updatedCacheEntry = new CacheEntry<T>()
                            {
                                CreatedOn = cachedValue.CreatedOn,
                                Value = cachedValue.Value,
                                State = CachedValueState.FoundButUpdating
                            };

                            await this.updatingCache.InsertValueAsync(key, updatedCacheEntry, this.expiresAfter);

                            return updatedCacheEntry;
                        }
                    }

                    return cachedValue;
                }
                else
                {
                    var databaseResult = await this.longTermCache.TryGetValueAsync<T>(key);
                    if (databaseResult.Found)
                    {
                        var newMemoryCacheEntry = new CacheEntry<T>()
                        {
                            CreatedOn = DateTime.Now,
                            State = CachedValueState.FoundAndNotUpdating,
                            CompletionProgress = 1.0,
                            Value = databaseResult.Result
                        };

                        await this.updatingCache.InsertValueAsync(key, newMemoryCacheEntry, expiresAfter);
                        return newMemoryCacheEntry;
                    }

                    var newUpdatingEntry = new CacheEntry<T>()
                    {
                        CreatedOn = DateTime.Now,
                        State = CachedValueState.Updating
                    };

                    Progress<double> progress = new Progress<double>();
                    progress.ProgressChanged += async (sender, e) =>
                    {
                        var memoryEntry = await this.updatingCache.TryGetValueAsync<CacheEntry<T>>(key);
                        memoryEntry.Result.CompletionProgress = e;
                    };

                    await taskRunner(async (token, serviceProvider) =>
                    {
                        using (var serviceScope = serviceProvider.CreateScope())
                        {
                            var cache = serviceScope.ServiceProvider.GetService<IDatabaseCache>();

                            var result = await valueRetriever(serviceProvider, progress);
                            await this.InsertData(cache, key, result);
                            return result;
                        }
                    });

                    await this.updatingCache.InsertValueAsync(key, newUpdatingEntry, this.expiresAfter);

                    return newUpdatingEntry;
                }
            }
            finally
            {
                semaphore.Release();
            }
        }

        public async Task RemoveCacheItem(string key)
        {
            await this.updatingCache.RemoveAsync(key);
            await this.longTermCache.RemoveAsync(key);
        }

        private async Task InsertData(ICache longTermCache, string key, T newData)
        {
            await semaphore.WaitAsync();

            try
            {
                await longTermCache.InsertValueAsync(key, newData, this.expiresAfter);

                var updatedCacheEntry = new CacheEntry<T>()
                {
                    CreatedOn = DateTime.Now,
                    State = CachedValueState.FoundAndNotUpdating,
                    Value = newData
                };

                await this.updatingCache.InsertValueAsync(key, updatedCacheEntry, this.expiresAfter);
            }
            finally
            {
                semaphore.Release();
            }
        }
    }
}
