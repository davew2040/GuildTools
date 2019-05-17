using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuildTools.Services.Mail
{
    public class ResetPasswordEmail
    {
        public string Subject { get; set; }
        public string TextContent { get; set; }
        public string HtmlContent { get; set; }
    }

    public class RegistrationConfirmationEmail
    {
        public string Subject { get; set; }
        public string TextContent { get; set; }
        public string HtmlContent { get; set; }
    }

    public class StatsGenerationComplete
    {
        public string Subject { get; set; }
        public string TextContent { get; set; }
        public string HtmlContent { get; set; }
    }
}
