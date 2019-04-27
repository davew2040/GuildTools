using GuildTools.EF.Models.StoredBlizzardModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GuildTools.EF.Models
{
    public partial class PlayerAlt
    {
        public int Id { get; set; }

        [Required]
        public int PlayerMainId { get; set; }

        [Required]
        public int PlayerId { get; set; }

        [Required]
        public int ProfileId { get; set; }

        public virtual PlayerMain PlayerMain { get; set; }
        public virtual StoredPlayer Player { get; set; }
        public virtual GuildProfile Profile { get; set; }
    }
}
