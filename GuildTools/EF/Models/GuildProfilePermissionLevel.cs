using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GuildTools.EF.Models
{
    public class GuildProfilePermissionLevel
    {
        [Key]
        public int Id { get; set; }
        public string PermissionName { get; set; }

        public virtual IEnumerable<User_GuildProfilePermissions> User_GuildProfilePermissions { get; set; }
    }
}
