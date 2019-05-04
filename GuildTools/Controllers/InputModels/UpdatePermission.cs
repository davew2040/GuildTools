using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GuildTools.Controllers.InputModels
{
    public class UpdatePermission
    {
        public bool Delete { get; set; }
        public string UserId { get; set; }
        public int NewPermissionLevel { get; set; }
    }
}
