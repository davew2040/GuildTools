using GuildTools.EF.Models.StoredBlizzardModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GuildTools.EF.Models
{
    public partial class GuildProfile
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [MaxLength(150)]
        [Required]
        public string ProfileName { get; set; }

        [Required]
        public string CreatorId { get; set; }

        [Required]
        public int RealmId { get; set; }

        public int? CreatorGuildId { get; set; }

        public virtual UserWithData Creator { get; set; }
        public virtual IEnumerable<User_GuildProfilePermissions> User_GuildProfilePermissions { get; set; }
        public virtual StoredRealm Realm { get; set; }
        public virtual StoredGuild CreatorGuild { get; set; }
        public virtual ICollection<StoredGuild> Guilds { get; set; }
        public virtual ICollection<StoredPlayer> Players { get; set; }
        public virtual ICollection<PlayerMain> PlayerMains { get; set; }
        public virtual ICollection<PlayerAlt> PlayerAlts { get; set; }
        public virtual ICollection<PendingAccessRequest> AccessRequests { get; set; }
    }
}
