using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GuildTools.EF.Models
{
    public partial class ValueStore
    {
        [Key]
        public string Id { get; set; }
        public string Value { get; set; }
    }
}
