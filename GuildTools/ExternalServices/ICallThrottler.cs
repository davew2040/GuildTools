using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Timers = System.Timers;

namespace GuildTools.ExternalServices
{
    public interface ICallThrottler
    {
        Task Throttle(Func<Task> action);
        Task<T> Throttle<T>(Func<Task<T>> action);
    }
}
