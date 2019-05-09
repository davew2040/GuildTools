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
using GuildTools.Services;
using GuildTools.EF.Models.StoredBlizzardModels;
using GuildTools.EF.Models;
using GuildTools.Data;
using GuildTools.Cache.SpecificCaches.CacheInterfaces;
using Microsoft.Extensions.DependencyInjection;
using Moq;

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
        public async Task TestRealmsGetter()
        {
            const string connectionString = "Data Source=(LocalDb)\\MSSQLLocalDB;Initial Catalog=GuildTools;Integrated Security=True";
            KeyedResourceManager manager = new KeyedResourceManager();

            BlizzardApiSecrets blizzardSecrets = new BlizzardApiSecrets()
            {
                ClientId = this.config.GetValue<string>("BlizzardApiSecrets:ClientId"),
                ClientSecret = this.config.GetValue<string>("BlizzardApiSecrets:ClientSecret")
            };

            IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
            GuildToolsContext context = new GuildToolsContext(SqlServerDbContextOptionsExtensions.UseSqlServer(new DbContextOptionsBuilder(), connectionString).Options as DbContextOptions);
            DatabaseCache dbCache = new DatabaseCache(context);

            IDataRepository repo = new DataRepository(context);
            IBlizzardService blizzardService = new BlizzardService(repo, this.config);

            RealmsCache cache = new RealmsCache(blizzardService, memoryCache, dbCache, manager);

            DateTime initial = DateTime.Now;

            var realms = await cache.GetRealms(EF.Models.Enums.GameRegionEnum.US);

            DateTime second = DateTime.Now;
            TimeSpan sinceInitial = second - initial;

            var realms2 = await cache.GetRealms(EF.Models.Enums.GameRegionEnum.US);

            DateTime third = DateTime.Now;
            TimeSpan sinceSecond = third - second;

            int x = 42;
        }

        [TestMethod]
        public async Task CachedStoredValueTest()
        {
            const string connectionString = "Data Source=(LocalDb)\\MSSQLLocalDB;Initial Catalog=GuildTools;Integrated Security=True";
            KeyedResourceManager manager = new KeyedResourceManager();

            BlizzardApiSecrets blizzardSecrets = new BlizzardApiSecrets()
            {
                ClientId = this.config.GetValue<string>("BlizzardApiSecrets:ClientId"),
                ClientSecret = this.config.GetValue<string>("BlizzardApiSecrets:ClientSecret")
            };

            IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
            GuildToolsContext context = new GuildToolsContext(SqlServerDbContextOptionsExtensions.UseSqlServer(new DbContextOptionsBuilder(), connectionString).Options as DbContextOptions);

            IDataRepository repo = new DataRepository(context);
            IBlizzardService blizzardService = new BlizzardService(repo, this.config);
            CallThrottler throttler = new CallThrottler(TimeSpan.FromSeconds(1));
            IGuildService guildService = new GuildService(blizzardService, throttler);

            RealmStoreByValues store = new RealmStoreByValues(guildService, memoryCache, context, manager);

            DateTime initial = DateTime.Now;

            var realms = await store.GetRealmAsync("Burning Blade", EF.Models.Enums.GameRegionEnum.US);

            DateTime second = DateTime.Now;
            TimeSpan sinceInitial = second - initial;

            var realms2 = await store.GetRealmAsync("Burning Blade", EF.Models.Enums.GameRegionEnum.US);

            DateTime third = DateTime.Now;
            TimeSpan sinceSecond = third - second;

            var realms3 = await store.GetRealmAsync("Akama", EF.Models.Enums.GameRegionEnum.US);

            DateTime fourth = DateTime.Now;
            TimeSpan sinceThird = fourth - third;

            int x = 42;
        }

        [TestMethod]
        public async Task PlayerStoreTest()
        {
            const string connectionString = "Data Source=(LocalDb)\\MSSQLLocalDB;Initial Catalog=GuildTools;Integrated Security=True";
            KeyedResourceManager manager = new KeyedResourceManager();

            BlizzardApiSecrets blizzardSecrets = new BlizzardApiSecrets()
            {
                ClientId = this.config.GetValue<string>("BlizzardApiSecrets:ClientId"),
                ClientSecret = this.config.GetValue<string>("BlizzardApiSecrets:ClientSecret")
            };

            IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
            GuildToolsContext context = new GuildToolsContext(SqlServerDbContextOptionsExtensions.UseSqlServer(new DbContextOptionsBuilder(), connectionString).Options as DbContextOptions);

            IDataRepository repo = new DataRepository(context);
            IBlizzardService blizzardService = new BlizzardService(repo, this.config);
            CallThrottler throttler = new CallThrottler(TimeSpan.FromSeconds(1));
            IGuildService guildService = new GuildService(blizzardService, throttler);

            int profileId = 1;

            GameRegion region = new GameRegion()
            {
                Id = 1,
                RegionName = "US"
            };

            StoredRealm realm = context.StoredRealms.Include(a => a.Region).First();
            StoredGuild guild = context.StoredGuilds.First();

            PlayerStoreByValue playerStore = new PlayerStoreByValue(guildService, memoryCache, context, manager);

            DateTime initial = DateTime.Now;

            var realms = await playerStore.GetPlayerAsync("Kromp", realm, guild, profileId);

            DateTime second = DateTime.Now;
            TimeSpan sinceInitial = second - initial;

            var realms2 = await playerStore.GetPlayerAsync("Kromp", realm, guild, profileId);

            DateTime third = DateTime.Now;
            TimeSpan sinceSecond = third - second;

            var realms3 = await playerStore.GetPlayerAsync("Kromp", realm, guild, profileId);

            DateTime fourth = DateTime.Now;
            TimeSpan sinceThird = fourth - third;

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

        [TestMethod]
        public async Task LongRunningCacheTest()
        {
            MemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
            LongRunningCache<int> cache = new LongRunningCache<int>(memoryCache, TimeSpan.FromSeconds(4), TimeSpan.FromSeconds(2));

            Debug.WriteLine("Starting...");

            var result = await cache.GetOrRefreshCachedValueAsync("testKey", this.testRunner(), this.immediateValueGetter());

            Debug.WriteLine(result.State);
            Assert.IsTrue(result.State == CachedValueState.Updating);

            await Task.Delay(1000);

            result = await cache.GetOrRefreshCachedValueAsync("testKey", this.testRunner(), this.immediateValueGetter());

            Debug.WriteLine(result.State);
            Assert.IsTrue(result.State == CachedValueState.FoundAndNotUpdating);

            await Task.Delay(2000);

            result = await cache.GetOrRefreshCachedValueAsync("testKey", this.testRunner(), this.immediateValueGetter());

            Debug.WriteLine("#3: " + result.State);
            Assert.IsTrue(result.State == CachedValueState.FoundButUpdating);

            await Task.Delay(1000);

            result = await cache.GetOrRefreshCachedValueAsync("testKey", this.testRunner(), this.immediateValueGetter());

            Debug.WriteLine("#4: " + result.State);
            Assert.IsTrue(result.State == CachedValueState.FoundAndNotUpdating);

            await Task.Delay(5000);

            result = await cache.GetOrRefreshCachedValueAsync("testKey", this.testRunner(), this.immediateValueGetter());

            Debug.WriteLine("#5: " + result.State);
            Assert.IsTrue(result.State == CachedValueState.Updating);
        }

        [TestMethod]
        public async Task LongRunningCacheTest_NoRefresh_InstantGet()
        {
            MemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
            LongRunningCache<int> cache = new LongRunningCache<int>(memoryCache, TimeSpan.FromSeconds(2));

            Debug.WriteLine("Starting...");

            var result = await cache.GetOrRefreshCachedValueAsync("testKey", this.testRunner(), this.immediateValueGetter());

            Debug.WriteLine(result.State);
            Assert.IsTrue(result.State == CachedValueState.Updating);

            await Task.Delay(1000);

            result = await cache.GetOrRefreshCachedValueAsync("testKey", this.testRunner(), this.immediateValueGetter());

            Debug.WriteLine(result.State);
            Assert.IsTrue(result.State == CachedValueState.FoundAndNotUpdating);

            await Task.Delay(3000);

            result = await cache.GetOrRefreshCachedValueAsync("testKey", this.testRunner(), this.immediateValueGetter());

            Debug.WriteLine("#3: " + result.State);
            Assert.IsTrue(result.State == CachedValueState.Updating);

            result = await cache.GetOrRefreshCachedValueAsync("testKey", this.testRunner(), this.immediateValueGetter());

            await Task.Delay(500);

            Debug.WriteLine("#4: " + result.State);
            Assert.IsTrue(result.State == CachedValueState.FoundAndNotUpdating);
        }

        [TestMethod]
        public async Task LongRunningCacheTest_WithRefresh_WaitingGet()
        {
            MemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
            LongRunningCache<int> cache = new LongRunningCache<int>(memoryCache, TimeSpan.FromSeconds(6), TimeSpan.FromSeconds(3));

            DateTime startTime = DateTime.Now;
            Debug.WriteLine("Starting...");

            var result = await cache.GetOrRefreshCachedValueAsync("testKey", this.testRunner(), this.waitingValueGetter(500));

            Debug.WriteLine("0.00: " + result.State);
            Assert.IsTrue(result.State == CachedValueState.Updating);

            await Task.Delay(250);
            result = await cache.GetOrRefreshCachedValueAsync("testKey", this.testRunner(), this.waitingValueGetter(500));

            Debug.WriteLine((DateTime.Now - startTime).TotalMilliseconds + ": " + result.State);
            Assert.IsTrue(result.State == CachedValueState.Updating);

            await Task.Delay(1000);
            result = await cache.GetOrRefreshCachedValueAsync("testKey", this.testRunner(), this.waitingValueGetter(500));

            Debug.WriteLine((DateTime.Now - startTime).TotalMilliseconds + ": " + result.State);
            Assert.IsTrue(result.State ==CachedValueState.FoundAndNotUpdating);

            await Task.Delay(3000);
            result = await cache.GetOrRefreshCachedValueAsync("testKey", this.testRunner(), this.waitingValueGetter(500));

            Debug.WriteLine((DateTime.Now - startTime).TotalMilliseconds + ": " + result.State);
            Assert.IsTrue(result.State == CachedValueState.FoundButUpdating);

            await Task.Delay(1000);
            result = await cache.GetOrRefreshCachedValueAsync("testKey", this.testRunner(), this.waitingValueGetter(500));

            Debug.WriteLine((DateTime.Now - startTime).TotalMilliseconds + ": " + result.State);
            Assert.IsTrue(result.State == CachedValueState.FoundAndNotUpdating);


            await Task.Delay(10000);
            result = await cache.GetOrRefreshCachedValueAsync("testKey", this.testRunner(), this.waitingValueGetter(500));

            Debug.WriteLine((DateTime.Now - startTime).TotalMilliseconds + ": " + result.State);
            Assert.IsTrue(result.State == CachedValueState.Updating);
        }

        private Func<Func<CancellationToken, IServiceProvider, Task<int>>, Task> testRunner()
        {
            Mock<IServiceProvider> serviceProvider = new Mock<IServiceProvider>();

            return async (runner) =>
            {
                await Task.Run(() =>
                {
                    runner(new CancellationToken(), serviceProvider.Object);
                });
            }; 
        }

        private Func<IProgress<double>, Task<int>> immediateValueGetter()
        {
            return async (progress) =>
            {
                return 42;
            };
        }

        private Func<IProgress<double>, Task<int>> waitingValueGetter(int milliseconds)
        {
            return async (progress) =>
            {
                await Task.Delay(milliseconds);
                return 42;
            };
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
