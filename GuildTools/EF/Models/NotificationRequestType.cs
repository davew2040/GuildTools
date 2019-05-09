using GuildTools.EF.Models.StoredBlizzardModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GuildTools.EF.Models
{
    public class NotificationRequestType
    {
        [Key]
        public int Id { get; set; }

        public string RequestTypeName { get; set; }

        public virtual ICollection<NotificationRequest> Requests { get; set; }
    }
}
