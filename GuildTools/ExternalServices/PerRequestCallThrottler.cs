using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Timers = System.Timers;

namespace GuildTools.ExternalServices
{
    public class PerRequestCallThrottler : ICallThrottler
    {
        private TimeSpan timeBetweenCalls;

        private SemaphoreSlim semaphore;
        private Timers.Timer releaseTimer;

        public PerRequestCallThrottler(TimeSpan timeBetweenCalls)
        {
            this.timeBetweenCalls = timeBetweenCalls;
            this.semaphore = new SemaphoreSlim(1);

            if (this.timeBetweenCalls.TotalMilliseconds > 0)
            {
                this.releaseTimer = new Timers.Timer(this.timeBetweenCalls.TotalMilliseconds);
                this.releaseTimer.AutoReset = false;
                this.releaseTimer.Elapsed += ReleaseTimer_Elapsed;
            }
        }

        public async Task Throttle(Func<Task> action)
        {
            if (this.timeBetweenCalls.TotalMilliseconds == 0)
            {
                await action();
                return;
            }

            await this.semaphore.WaitAsync();

            await action();

            this.releaseTimer.Start();
        }

        public async Task<T> Throttle<T>(Func<Task<T>> action)
        {
            if (this.timeBetweenCalls.TotalMilliseconds == 0)
            {
                return await action();
            }

            await this.semaphore.WaitAsync();

            var result = await action();

            this.releaseTimer.Start();

            return result;
        }

        private void ReleaseTimer_Elapsed(object sender, Timers.ElapsedEventArgs e)
        {
            this.semaphore.Release();
        }
    }
}
