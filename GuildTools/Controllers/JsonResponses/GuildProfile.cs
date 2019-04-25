using System;
using System.Collections.Generic;

namespace GuildTools.Controllers.JsonResponses
{
    public class GuildProfile
    {
        public int Id { get; set; }
        public string GuildName { get; set; }
        public string Realm { get; set; }
        public string Region { get; set; }
    }
}
