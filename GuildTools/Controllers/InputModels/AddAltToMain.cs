using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuildTools.Controllers.InputModels
{
    public class AddAltToMain
    {
        public int ProfileId { get; set; }
        public int MainId { get; set; }
        public int PlayerId { get; set; }
    }
}
