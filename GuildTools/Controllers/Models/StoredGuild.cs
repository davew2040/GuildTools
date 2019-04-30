using System;
using System.Collections.Generic;

namespace GuildTools.Controllers.Models
{
    public class StoredGuild
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int RealmId { get; set; }
        public IEnumerable<BlizzardPlayer> Players { get;set; }
    }
}
