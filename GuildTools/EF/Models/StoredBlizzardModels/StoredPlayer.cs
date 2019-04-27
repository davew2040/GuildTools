using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GuildTools.EF.Models.StoredBlizzardModels
{
    public partial class StoredPlayer
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public int RealmId { get; set; }

        [Required]
        public int ProfileId { get; set; }

        public virtual StoredRealm Realm { get; set; }
        public virtual GuildProfile Profile { get; set; }
        public virtual PlayerMain Main { get; set; }
        public virtual PlayerAlt Alt { get; set; }
    }
}
