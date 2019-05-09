using GuildTools.EF.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuildTools.Controllers.InputModels
{
    public class RequestStatsCompleteNotification
    {
        public string Email { get; set; }
        public string Region { get; set; }
        public string Guild { get; set; }
        public string Realm { get; set; }
        public int RequestType { get; set; }
    }
}