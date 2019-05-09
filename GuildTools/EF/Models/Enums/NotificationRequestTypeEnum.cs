using GuildTools.EF.Models.StoredBlizzardModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GuildTools.EF.Models.Enums
{
    public enum NotificationRequestTypeEnum

    {
        StatsRequestComplete = 1,
        RaiderIoStatsRequestComplete = 2
    }
}
