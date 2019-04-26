﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GuildTools.EF.Models
{
    public partial class GuildProfile
    {
        public int Id { get; set; }
        [MaxLength(150)]
        [Required]
        public string ProfileName { get; set; }
        public string GuildName { get; set; }
        public string Realm { get; set; }
        public string CreatorId { get; set; }
        public int RegionId { get; set; }

        public virtual AspNetUsers Creator { get; set; }
        public virtual IEnumerable<User_GuildProfilePermissions> User_GuildProfilePermissions { get; set; }
        public virtual GameRegion Region { get; set; }
    }
}