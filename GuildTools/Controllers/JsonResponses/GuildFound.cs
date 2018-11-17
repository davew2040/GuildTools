using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetAngularTemplate.Controllers.JsonResponses
{
    public class GuildFound
    {
        public bool Found { get; set; }
        public string Realm { get; set; }
        public string Name { get; set; }
    }
}
