using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuildTools.Services
{
    public interface ICommonValuesProvider
    {
        string AdminEmail { get; set; }
        string AdminName { get; set; }
    }
}
