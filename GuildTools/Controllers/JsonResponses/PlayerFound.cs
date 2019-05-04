using GuildTools.Controllers.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuildTools.Controllers.JsonResponses
{
    public class PlayerFound
    {
        public bool Found { get; set; }
        public BlizzardPlayer PlayerDetails { get; set; }
    }
}
