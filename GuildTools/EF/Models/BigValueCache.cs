using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GuildTools.EF.Models
{
    public partial class BigValueCache
    {
        [Key]
        public string Id { get; set; }
        public string Value { get; set; }
        public DateTime ExpiresOn { get; set; }
    }
}
