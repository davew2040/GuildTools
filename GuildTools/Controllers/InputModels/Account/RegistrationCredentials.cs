using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GuildTools.Models
{
    public class RegistrationDetails
    {
        [Required]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public string PlayerName { get; set; }

        public string PlayerRealm { get; set; }

        public string GuildName { get; set; }

        public string GuildRealm { get; set; }

        public string PlayerRegion { get; set; }
    }
}
