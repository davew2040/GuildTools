using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GuildTools.EF.Models
{
    public partial class FriendGuild
    {
        [Key]
        public int Id { get; set; }
        public int StoredGuildId { get; set; }
        public int ProfileId { get; set; }

        public virtual StoredBlizzardModels.StoredGuild Guild { get; set; }
        public virtual GuildProfile Profile { get; set; }
    }
}
