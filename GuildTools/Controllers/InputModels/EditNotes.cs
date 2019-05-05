using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GuildTools.Controllers.InputModels
{
    public class EditNotes
    {
        public int ProfileId { get; set; }
        public int PlayerMainId { get; set; }
        public string NewNotes { get; set; }
    }
}
