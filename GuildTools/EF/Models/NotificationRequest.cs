using GuildTools.EF.Models.StoredBlizzardModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GuildTools.EF.Models
{
    public partial class NotificationRequest
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Email { get; set; }

        public string OperationKey { get; set; }
        
        [Required]
        public int NotificationRequestTypeId { get; set; }

        public virtual NotificationRequestType RequestType { get; set; }
    }
}
