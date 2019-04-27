using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GuildTools.EF.Models.StoredBlizzardModels
{
    public partial class StoredGuild
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public int RealmId { get; set; }

        [Required]
        public int ProfileId { get; set; }

        public GuildProfile Profile { get; set; }
        public StoredRealm Realm { get; set; }
    }
}
