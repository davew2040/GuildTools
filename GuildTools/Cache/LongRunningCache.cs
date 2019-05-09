using Microsoft.Extensions.Caching.Memory;
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

        private IMemoryCache cache;
        private TimeSpan expiresAfter;
        private TimeSpan? refreshAfter;
        private SemaphoreSlim semaphore = new SemaphoreSlim(1);

        public LongRunningCache(IMemoryCache cache, TimeSpan expiresAfter, TimeSpan? refreshAfter = null)
        {
            this.cache = cache;

            if (expiresAfter < refreshAfter)
            {
                throw new ArgumentException("Refreshes must occur before expiration!");
            }

            this.expiresAfter = expiresAfter;
            this.refreshAfter = refreshAfter;
        }

        public async Task<CacheEntry<T>> GetOrRefreshCachedValueAsync(
            string key, 
            Func<Func<CancellationToken, IServiceProvider, Task<T>>, Task> taskRunner, Func<IProgress<double>, 
            Task<T>> valueRetriever)
        {
            await semaphore.WaitAsync();

            try
            {
                CacheEntry<T> cachedValue = null;

                if (this.cache.TryGetValue(key, out cachedValue))
                {
                    if (this.refreshAfter.HasValue)
                    {
                        if (DateTime.Now > cachedValue.CreatedOn + this.refreshAfter.Value
                            && cachedValue.State == CachedValueState.FoundAndNotUpdating)
                        {
                            await taskRunner(async (token, serviceProvider) =>
                            {
                                var result = await valueRetriever(new Progress<double>());
                                this.InsertData(key, result);
                                return result;
                            });

                            var updatedCacheEntry = new CacheEntry<T>()
                            {
                                CreatedOn = cachedValue.CreatedOn,
                                Value = cachedValue.Value,
                                State = CachedValueState.FoundButUpdating
                            };

                            this.cache.Set(key, updatedCacheEntry, this.expiresAfter);

                            return updatedCacheEntry;
                        }
                    }

                    return cachedValue;
                }
                else
                {
                    var newCacheEntry = new CacheEntry<T>()
                    {
                        CreatedOn = DateTime.Now,
                        State = CachedValueState.Updating
                    };

                    Progress<double> progress = new Progress<double>();
                    progress.ProgressChanged += (sender, e) =>
                    {
                        this.cache.Get<CacheEntry<T>>(key).CompletionProgress = e;
                    };

                    await taskRunner(async (token, serviceProvider) =>
                    {
                        var result = await valueRetriever(progress);
                        this.InsertData(key, result);
                        return result;
                    });

                    this.cache.Set(key, newCacheEntry, this.expiresAfter);

                    return newCacheEntry;
                }
            }
            finally
            {
                semaphore.Release();
            }
        }



        private void InsertData(string key, T newData)
        {
            semaphore.WaitAsync();

            try
            {
                var updatedCacheEntry = new CacheEntry<T>()
                {
                    CreatedOn = DateTime.Now,
                    State = CachedValueState.FoundAndNotUpdating,
                    Value = newData
                };

                this.cache.Set(key, updatedCacheEntry, this.expiresAfter);
            }
            finally
            {
                semaphore.Release();
            }
        }
    }
}
