using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GuildTools.EF.Models.StoredBlizzardModels
{
    public partial class StoredGuild
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

        public virtual GuildProfile Profile { get; set; }
        public virtual StoredRealm Realm { get; set; }
        public virtual ICollection<GuildProfile> CreatorGuilds { get; set; }
        public virtual ICollection<StoredPlayer> StoredPlayers { get; set; }
    }
}
