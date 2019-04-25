using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuildTools.Services
{
    public class CommonValuesProvider : ICommonValuesProvider
    {
        public string AdminEmail { get; set; }
        public string AdminName { get; set; }
    }
}
