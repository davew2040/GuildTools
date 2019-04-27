using GuildTools.EF.Models.StoredBlizzardModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GuildTools.EF.Models
{
    public partial class PlayerMain
    {
        public int Id { get; set; }

        [Required]
        public int ProfileId { get; set; }

        [Required]
        public int PlayerId { get; set; }

        [MaxLength(4000)]
        public string Notes { get; set; }

        [MaxLength(4000)]
        public string OfficerNotes { get; set; }

        public virtual GuildProfile Profile { get; set; }
        public virtual StoredPlayer Player { get; set; }
        public virtual ICollection<PlayerAlt> Alts { get; set; }
    }
}
