using GuildTools.ExternalServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace GuildTools.Tests
{
    [TestClass]
    public class CallThrottlerTests
    {
        [TestMethod]
        public async Task TestThrottler()
        {
            var throttler = new PerRequestCallThrottler(TimeSpan.FromSeconds(3));

            var tasks = new List<Task>();

            DateTime starting = DateTime.Now;

            List<string> buffers = new List<string>();

            tasks.Add(Task.Run(new Func<Task>(async () =>
            {
                await throttler.Throttle(async () => { buffers.Add("one: " + (DateTime.Now-starting).TotalMilliseconds); });
            })));

            tasks.Add(Task.Run(new Func<Task>(async () =>
            {
                await throttler.Throttle(async () => { buffers.Add("two: " + (DateTime.Now - starting).TotalMilliseconds); });
            })));

            tasks.Add(Task.Run(new Func<Task>(async () =>
            {
                await throttler.Throttle(async () => { buffers.Add("three: " + (DateTime.Now - starting).TotalMilliseconds); });
            })));

            await Task.WhenAll(tasks);

            Debug.WriteLine(string.Join(",", buffers));
        }

        [TestMethod]
        public async Task TestThrottlerWithReturnType()
        {
            var throttler = new PerRequestCallThrottler(TimeSpan.FromSeconds(3));

            var tasks = new List<Task>();

            DateTime starting = DateTime.Now;

            List<string> buffers = new List<string>();

            tasks.Add(Task.Run(new Func<Task>(async () =>
            {
                buffers.Add(await throttler.Throttle(async () => { return "one: " + (DateTime.Now - starting).TotalMilliseconds; }));
            })));

            tasks.Add(Task.Run(new Func<Task>(async () =>
            {
                buffers.Add(await throttler.Throttle(async () => { return "two: " + (DateTime.Now - starting).TotalMilliseconds; }));
            })));

            tasks.Add(Task.Run(new Func<Task>(async () =>
            {
                buffers.Add(await throttler.Throttle(async () => { return "three: " + (DateTime.Now - starting).TotalMilliseconds; }));
            })));

            await Task.WhenAll(tasks);

            Debug.WriteLine(string.Join(",", buffers));
        }
    }
}
