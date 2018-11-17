using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuildTools.Sql.SqlModels
{
    public class CachedValue
    {
        public string Key { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
        public DateTime ExpiresOn { get; set; }
    }
}
