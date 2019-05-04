using System;
using System.Collections.Generic;

namespace GuildTools.Controllers.Models
{
    public class LoginResponse
    {
        public Dictionary<string, object> AuthenticationDetails { get; set; }
        public string Username;
        public string Email;
        public string GuildName;
        public string GuildRealm;
        public string PlayerName;
        public string PlayerRealm;
        public string PlayerRegion;
    }
}
