using System;
using System.Collections.Generic;

namespace GuildTools.Controllers.Models
{
    public class FriendGuild
    {
        public int Id { get; set; }
        public StoredGuild Guild { get; set; }
    }
}
