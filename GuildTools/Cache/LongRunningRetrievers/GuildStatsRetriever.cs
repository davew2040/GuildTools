﻿using GuildTools.Cache.LongRunningRetrievers.Interfaces;
using GuildTools.Controllers.JsonResponses;
using GuildTools.Data;
using GuildTools.EF.Models.Enums;
using GuildTools.ExternalServices.Blizzard;
using GuildTools.ExternalServices.Blizzard.Utilities;
using GuildTools.Scheduler;
using GuildTools.Services;
using GuildTools.Services.Mail;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GuildTools.Cache;
using static GuildTools.ExternalServices.Blizzard.BlizzardService;

namespace GuildTools.Cache.LongRunningRetrievers
{
    public class GuildStatsRetriever : IGuildStatsRetriever
    {
        private readonly LongRunningCache<IEnumerable<GuildMemberStats>> longRunningCache;
        private readonly IBackgroundTaskQueue taskQueue;
        private readonly IMailSender mailSender;
        private readonly ICommonValuesProvider commonValues;
        private readonly IMailGenerator mailGenerator;

        public GuildStatsRetriever(
            ICache memoryCache,
            IDatabaseCache databaseCache,
            IBackgroundTaskQueue taskQueue,
            IMailSender mailSender,
            ICommonValuesProvider commonValues,
            IMailGenerator mailGenerator)
        {
            this.longRunningCache = new LongRunningCache<IEnumerable<GuildMemberStats>>(memoryCache, databaseCache, TimeSpan.FromDays(3), TimeSpan.FromDays(1));
            this.taskQueue = taskQueue;
            this.mailSender = mailSender;
            this.commonValues = commonValues;
            this.mailGenerator = mailGenerator;
        }

        public async Task<CacheEntry<IEnumerable<GuildMemberStats>>> GetCachedEntry(BlizzardRegion region, string realm, string guild)
        {
            string key = this.GetKey(region, realm, guild);

            return await this.longRunningCache.GetOrRefreshCachedValueAsync(
                key,
                this.taskRunner(key, region, realm, guild),
                this.valueGetter(region, realm, guild));
        }

        string IGuildStatsRetriever.GetKey(BlizzardRegion region, string realm, string guild)
        {
            string key = this.GetKey(region, realm, guild);

            return key;
        }

        public int? GetPositionInQueue(BlizzardRegion region, string realm, string guild)
        {
            string key = this.GetKey(region, realm, guild);

            return this.taskQueue.FindItemPlaceInQueue(key);
        }

        private Func<Func<CancellationToken, IServiceProvider, Task<IEnumerable<GuildMemberStats>>>, Task> taskRunner(
            string key, BlizzardRegion region, string realm, string guild)
        {
            return async (runner) =>
            {
                this.taskQueue.QueueBackgroundWorkItem(new BackgroundWorkItem()
                {
                    Key = key,
                    Worker = async (token, serviceProvider) =>
                    {
                        await runner(token, serviceProvider);

                        await this.SendStatsCompleteNotifications(serviceProvider, key, region, realm, guild);
                    },
                    TaskFailed = async () =>
                    {
                        await this.longRunningCache.RemoveCacheItem(key);
                    }
                });
            };
        }

        private async Task SendStatsCompleteNotifications(IServiceProvider serviceProvider, string key, BlizzardRegion region, string realm, string guild)
        {
            using (var serviceScope = serviceProvider.CreateScope())
            {
                var repo = serviceScope.ServiceProvider.GetService<IDataRepository>();

                var notifications = await repo.GetAndClearNotifications(NotificationRequestTypeEnum.StatsRequestComplete, key);

                string url = $"guildstats/{region.ToString()}/{BlizzardService.FormatGuildName(guild)}/{BlizzardService.FormatRealmName(realm)}";

                List<Task> mailTasks = new List<Task>();

                var generatedEmail = this.mailGenerator.GenerateStatsCompleteEmail(url);

                foreach (var notification in notifications)
                {
                    mailTasks.Add(this.mailSender.SendMailAsync(
                        notification.Email,
                        this.commonValues.AdminEmail,
                        generatedEmail.Subject,
                        generatedEmail.TextContent,
                        generatedEmail.HtmlContent));
                }

                await Task.WhenAll(mailTasks);
            }
        }

        private Func<IServiceProvider, IProgress<double>, Task<IEnumerable<GuildMemberStats>>> valueGetter(BlizzardRegion region, string realm, string guild)
        {
            return async (serviceProvider, progress) =>
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    var service = scope.ServiceProvider.GetService<IGuildService>();
                    return await service.GetLargeGuildMembersDataAsync(region, guild, realm, progress);
                }
            };
        }

        private string GetKey(BlizzardRegion region, string realm, string guild)
        {
            var regionKey = Keyifier.GetRegionKey(BlizzardUtilities.GetEfRegionFromBlizzardRegion(region));
            var realmKey = Keyifier.GetRealmKey(realm);
            var guildKey = Keyifier.GetGuildKey(guild);

            return Keyifier.GetKey("blizzard_guildstats", new List<string> { regionKey, realmKey, guildKey });
        }
    }
}
