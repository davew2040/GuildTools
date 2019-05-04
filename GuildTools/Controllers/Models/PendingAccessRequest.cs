using System;
using System.Collections.Generic;

namespace GuildTools.Controllers.Models
{
    public class PendingAccessRequest
    {
        public int Id { get; set; }
        public int ProfileId { get; set; }
        public UserStub User { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
