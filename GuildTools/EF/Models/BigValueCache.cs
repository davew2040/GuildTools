using System;
using System.Collections.Generic;

namespace GuildTools.EF.Models
{
    public partial class BigValueCache
    {
        public string Id { get; set; }
        public string Value { get; set; }
        public DateTime ExpiresOn { get; set; }
    }
}
