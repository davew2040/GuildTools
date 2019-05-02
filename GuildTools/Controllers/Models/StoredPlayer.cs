using System;
using System.Collections.Generic;

namespace GuildTools.Controllers.Models
{
    public class StoredPlayer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Level { get; set; }
        public int Class { get; set; }
        public int GuildId { get; set; }
    }
}
