using System;
using System.Collections.Generic;

namespace GuildTools.Controllers.Models
{
    public class PlayerMain
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Notes { get; set; }
        public string OfficerNotes { get; set; }
        public int Level { get; set; }
        public int Class { get; set; }
        
        public IEnumerable<PlayerAlt> Alts { get; set; }
    }
}
