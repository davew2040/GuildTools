using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Timers = System.Timers;

namespace GuildTools.ExternalServices
{
    public class FixedIterationDelayCallThrottler : ICallThrottler
    {
        private TimeSpan delay;

        private TaskCompletionSource<object> resetEvent;
        private Timers.Timer releaseTimer;
        private int iterationCount;
        private readonly int iterations;
        private bool waiting = false;

        public FixedIterationDelayCallThrottler(int iterations, TimeSpan delay)
        {
            this.iterations = iterations;
            this.delay = delay;

            this.releaseTimer = new Timers.Timer(this.delay.TotalMilliseconds);
            this.releaseTimer.AutoReset = false;
            this.releaseTimer.Elapsed += ReleaseTimer_Elapsed;
        }

        public async Task Throttle(Func<Task> action)
        {
            lock (this)
            {
                iterationCount++;

                if (this.iterationCount >= this.iterations)
                {
                    waiting = true;
                    this.resetEvent = new TaskCompletionSource<object>();
                    this.releaseTimer.Start();
                    this.iterationCount = 0;
                }
            }

            if (waiting)
            {
                await this.resetEvent.Task;
            }

            Debug.WriteLine("Called at " + iterationCount);
            await action();
        }

        public async Task<T> Throttle<T>(Func<Task<T>> action)
        {
            //this.iterationCount++;

            //this.resetEvent.Wait();

            //if (this.iterationCount >= this.iterations)
            //{
            //    this.resetEvent.Reset();
            //    this.releaseTimer.Start();
            //    this.iterationCount = 0;
            //}

            var result = await action();

            //iterationCount++;

            return result;
        }

        private void ReleaseTimer_Elapsed(object sender, Timers.ElapsedEventArgs e)
        {
            waiting = false;
            this.resetEvent.TrySetResult(null);
        }
    }
}
