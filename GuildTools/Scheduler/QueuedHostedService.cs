using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GuildTools.Scheduler
{
    public class QueuedHostedService : BackgroundService
    {
        private readonly IServiceProvider serviceProvider;

        public QueuedHostedService(
            IBackgroundTaskQueue taskQueue,
            IServiceProvider serviceProvider)
        {
            TaskQueue = taskQueue;
            this.serviceProvider = serviceProvider;
        }

        public BackgroundWorkItem ActiveWorkItem { get; private set; }

        public IBackgroundTaskQueue TaskQueue { get; }

        protected async override Task ExecuteAsync(
            CancellationToken cancellationToken)
        {
            Log.Information("Queued Hosted Service is starting.");

            while (!cancellationToken.IsCancellationRequested)
            {
                var workItem = await TaskQueue.DequeueAsync(cancellationToken);
                this.ActiveWorkItem = workItem;

                try
                {
                    await workItem.Worker(cancellationToken, serviceProvider);
                    this.ActiveWorkItem = null;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"Error occurred executing item {workItem.Key}.");
                    await this.ActiveWorkItem.TaskFailed();
                }
            }

            Log.Information("Queued Hosted Service is starting.");
        }
    }
}
