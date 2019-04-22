using GuildTools.Cache;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GuildTools.EF;

namespace GuildTools.Tests
{
    [TestClass]
    public class CacheTests
    {
        [TestMethod]
        public async Task MemoryCache()
        {
            IMemoryCache cache = new MemoryCache(new MemoryCacheOptions() { ExpirationScanFrequency = TimeSpan.FromSeconds(1) });
            var entry = cache.CreateEntry("test").SetValue(3).SetAbsoluteExpiration(TimeSpan.FromSeconds(2));

            cache.Set("test", entry);

            cache.TryGetValue("test", out ICacheEntry outInt);

            Assert.AreEqual(3, outInt.Value);

            await Task.Delay(4000);

            cache.TryGetValue("test", out ICacheEntry outInt2);

            Assert.AreEqual(3, outInt2.Value);
        }

        [TestMethod]
        public async Task TestKeyedResource()
        {
            const string connectionString = "Data Source=(LocalDb)\\MSSQLLocalDB;Initial Catalog=GuildTools;Integrated Security=True";
            KeyedResourceManager manager = new KeyedResourceManager();

            IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
            GuildToolsContext context = new GuildToolsContext(SqlServerDbContextOptionsExtensions.UseSqlServer(new DbContextOptionsBuilder(), connectionString).Options as DbContextOptions);
            DatabaseCache<string> dbCache = new DatabaseCache<string>(context, TimeSpan.FromSeconds(10));
            DatabaseCacheWithMemoryCache<string> cache = new DatabaseCacheWithMemoryCache<string>(TimeSpan.FromSeconds(5), dbCache, memoryCache);

            TestKeyedResource keyedResource = new TestKeyedResource(manager, cache);

            IEnumerable<Task<string>> tasks = new List<Task<string>>()
            {
                keyedResource.Get("bah")
            };

            Task.WaitAll(tasks.ToArray());

            var results = tasks.Select(t => t.Result);

            int x = 42;
        }

        private async Task<string> SingleThreadTest(SemaphoreSlim sem)
        {
           sem.Wait();

            await Task.Delay(2000);

           sem.Release();

            return DateTime.Now.ToString();
        }
    }
}
