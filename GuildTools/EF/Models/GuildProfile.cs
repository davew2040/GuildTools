using System;
using System.Collections.Generic;

namespace GuildTools.EF.Models
{
    public partial class GuildProfile
    {
        public int Id { get; set; }
        public string GuildName { get; set; }
        public string Realm { get; set; }
        public string CreatorId { get; set; }

        public virtual AspNetUsers Creator { get; set; }
    }
}
