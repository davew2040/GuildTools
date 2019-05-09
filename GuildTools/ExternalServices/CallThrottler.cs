using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Timers = System.Timers;

namespace GuildTools.ExternalServices
{
    public class CallThrottler : ICallThrottler
    {
        private TimeSpan timeBetweenCalls;

        private SemaphoreSlim semaphore;
        private Timers.Timer releaseTimer;

        public CallThrottler(TimeSpan timeBetweenCalls)
        {
            this.timeBetweenCalls = timeBetweenCalls;
            this.semaphore = new SemaphoreSlim(1);

            this.releaseTimer = new Timers.Timer(this.timeBetweenCalls.TotalMilliseconds);
            this.releaseTimer.AutoReset = false;
            this.releaseTimer.Elapsed += ReleaseTimer_Elapsed;
        }

        public async Task Throttle(Func<Task> action)
        {
            await this.semaphore.WaitAsync();

            await action();

            this.releaseTimer.Start();
        }

        public async Task<T> Throttle<T>(Func<Task<T>> action)
        {
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
