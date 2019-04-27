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
using GuildTools.ExternalServices;
using static GuildTools.ExternalServices.Blizzard.BlizzardService;
using GuildTools.ExternalServices.Blizzard;
using Microsoft.Extensions.Configuration;
using System.IO;
using GuildTools.Configuration;
using GuildTools.Cache.SpecificCaches;

namespace GuildTools.Tests
{
    [TestClass]
    public class CacheTests
    {
        private IConfiguration config;

        [TestInitialize]
        public void SetupTests()
        {
            var executingDirectory = Directory.GetCurrentDirectory();
            this.config = GetApplicationConfiguration(executingDirectory);
        }

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
        public async Task TetRealmsGetter()
        {
            const string connectionString = "Data Source=(LocalDb)\\MSSQLLocalDB;Initial Catalog=GuildTools;Integrated Security=True";
            KeyedResourceManager manager = new KeyedResourceManager();

            BlizzardApiSecrets blizzardSecrets = new BlizzardApiSecrets()
            {
                ClientId = this.config.GetValue<string>("BlizzardApiSecrets:ClientId"),
                ClientSecret = this.config.GetValue<string>("BlizzardApiSecrets:ClientSecret")
            };

            IBlizzardService blizzardService = new BlizzardService(connectionString, blizzardSecrets);
            IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
            GuildToolsContext context = new GuildToolsContext(SqlServerDbContextOptionsExtensions.UseSqlServer(new DbContextOptionsBuilder(), connectionString).Options as DbContextOptions);
            DatabaseCache dbCache = new DatabaseCache(context);

            RealmsCache cache = new RealmsCache(blizzardService, memoryCache, dbCache, manager);

            DateTime initial = DateTime.Now;

            var realms = await cache.GetRealms(EF.Models.Enums.GameRegion.US);

            DateTime second = DateTime.Now;
            TimeSpan sinceInitial = second - initial;

            var realms2 = await cache.GetRealms(EF.Models.Enums.GameRegion.US);

            DateTime third = DateTime.Now;
            TimeSpan sinceSecond = third - second;

            int x = 42;
        }


        private async Task<string> SingleThreadTest(SemaphoreSlim sem)
        {
           sem.Wait();

            await Task.Delay(2000);

           sem.Release();

            return DateTime.Now.ToString();
        }

        [TestMethod]
        public async Task MemoryCachePerfTest()
        {
            IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());

            memoryCache.Set("key", "value");

            var startTime = DateTime.Now;

            for (int i = 0; i < 10000000; i++)
            {
                SemaphoreSlim semaphore = new SemaphoreSlim(1);
                await semaphore.WaitAsync();
                string value;
                memoryCache.TryGetValue("key", out value);
                semaphore.Release();
            }

            var endTime = DateTime.Now;

            int x = 42;
        }

        public static IConfigurationRoot GetIConfigurationRoot(string outputPath)
        {
            return new ConfigurationBuilder()
                .SetBasePath(outputPath)
                .AddUserSecrets("57626c94-03e6-4856-86fc-3777d11151c4")
                .AddEnvironmentVariables()
                .Build();
        }

        public static IConfiguration GetApplicationConfiguration(string outputPath)
        {
            return GetIConfigurationRoot(outputPath);
        }
    }
}
