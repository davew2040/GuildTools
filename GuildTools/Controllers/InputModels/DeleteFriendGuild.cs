using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuildTools.Controllers.InputModels
{
    public class DeleteFriendGuild
    {
        public int ProfileId { get; set; }
        public int FriendGuildId { get; set; }
    }
}
