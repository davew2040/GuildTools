using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EfEnums = GuildTools.EF.Models.Enums;

namespace GuildTools.Data.RepositoryModels
{
    public class CachedValue
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public DateTime ExpiresOn { get; set; }
    }
}
