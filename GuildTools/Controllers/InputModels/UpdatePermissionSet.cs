using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GuildTools.Controllers.InputModels
{
    public class UpdatePermissionSet
    {
        public int ProfileId { get; set; }
        public IEnumerable<UpdatePermission> Updates { get; set; }
    }
}
