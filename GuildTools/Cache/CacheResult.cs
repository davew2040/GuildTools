using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuildTools.Cache
{
    public class CacheResult<T>
    {
        public bool Found { get; set; }
        public T Result { get; set; }
    }
}
