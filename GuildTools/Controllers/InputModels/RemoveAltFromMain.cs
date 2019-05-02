using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuildTools.Controllers.InputModels
{
    public class RemoveAltFromMain
    {
        public int AltId { get; set; }
        public int MainId { get; set; }
        public int ProfileId { get; set; }
    }
}
