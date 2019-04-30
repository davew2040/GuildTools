using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GuildTools.EF.Models.StoredBlizzardModels
{
    public partial class StoredRealm
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
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
        public virtual ICollection<GuildProfile> Profiles { get; set; }
    }
}
