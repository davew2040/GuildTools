using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Timers = System.Timers;

namespace GuildTools.ExternalServices
{
    public class ConcurrencyLimitedCallThrottler : ICallThrottler
    {
        private SemaphoreSlim semaphore;

        public ConcurrencyLimitedCallThrottler(int concurrentCallCount)
        {
            this.semaphore = new SemaphoreSlim(concurrentCallCount);
        }

        public async Task Throttle(Func<Task> action)
        {
            await this.semaphore.WaitAsync();

            await action();

            this.semaphore.Release();
        }

        public async Task<T> Throttle<T>(Func<Task<T>> action)
        {
            await this.semaphore.WaitAsync();

            var result = await action();

            this.semaphore.Release();

            return result;
        }
    }
}
