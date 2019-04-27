using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GuildTools.EF.Models.StoredBlizzardModels
{
    public partial class StoredRealm
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Slug { get; set; }

        [Required]
        public int RegionId { get; set; }
        
        public virtual GameRegion Region { get; set; }
        public virtual ICollection<StoredGuild> Guilds { get; set; }
        public virtual ICollection<StoredPlayer> Players { get; set; }
    }
}
