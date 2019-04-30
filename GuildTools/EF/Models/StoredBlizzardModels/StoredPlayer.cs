using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GuildTools.EF.Models.StoredBlizzardModels
{
    public partial class StoredPlayer
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public int RealmId { get; set; }

        [Required]
        public int ProfileId { get; set; }

        [Required]
        public int Level { get; set; }

        [Required]
        public int Class { get; set; }

        public int? GuildId { get; set; }

        public virtual StoredRealm Realm { get; set; }
        public virtual GuildProfile Profile { get; set; }
        public virtual PlayerMain Main { get; set; }
        public virtual PlayerAlt Alt { get; set; }
        public virtual StoredGuild Guild { get; set; }
    }
}
