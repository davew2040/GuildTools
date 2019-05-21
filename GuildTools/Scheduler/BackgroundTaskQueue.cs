using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GuildTools.Scheduler
{
    public class BackgroundWorkItem
    {
        public string Key { get; set; }
        public Func<CancellationToken, IServiceProvider, Task> Worker { get; set; }
        public Func<Task> TaskFailed { get; set; }
    }

    public interface IBackgroundTaskQueue
    {
        void QueueBackgroundWorkItem(BackgroundWorkItem workItem);

        Task<BackgroundWorkItem> DequeueAsync(
            CancellationToken cancellationToken);

        int? FindItemPlaceInQueue(string key);
    }

    public class BackgroundTaskQueue : IBackgroundTaskQueue
    {
        private ConcurrentQueue<BackgroundWorkItem> _workItems =
            new ConcurrentQueue<BackgroundWorkItem>();
        private SemaphoreSlim _signal = new SemaphoreSlim(0);

        public void QueueBackgroundWorkItem(BackgroundWorkItem workItem)
        {
            if (workItem == null)
            {
                throw new ArgumentNullException(nameof(workItem));
            }

            _workItems.Enqueue(workItem);
            _signal.Release();
        }

        public async Task<BackgroundWorkItem> DequeueAsync(
            CancellationToken cancellationToken)
        {
            await _signal.WaitAsync(cancellationToken);
            _workItems.TryDequeue(out var workItem);

            return workItem;
        }

        public int? FindItemPlaceInQueue(string key)
        {
            int place = 1;
            foreach (var item in _workItems)
            {
                if (item.Key == key)
                {
                    return place;
                }

                place++;
            }

            return null;
        }
    }
}
