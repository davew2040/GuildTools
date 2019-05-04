using GuildTools.EF.Models.StoredBlizzardModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GuildTools.EF.Models
{
    public partial class PendingAccessRequest
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string RequesterId { get; set; }

        [Required]
        public int ProfileId { get; set; }

        [Required]
        public DateTime CreatedOn { get; set; }

        public virtual GuildProfile Profile { get; set; }
        public virtual UserWithData Requester { get; set; }
    }
}
