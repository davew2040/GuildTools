using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuildTools.Services.Mail
{
    public class MailGenerator
    {
        public static ResetPasswordEmail GenerateResetPasswordEmail(string url)
        {
            var newEmail = new ResetPasswordEmail();

            newEmail.Subject = "Reset your GuildTools password:";

            newEmail.TextContent =
                $@"Hello from the GuildTools Team!

Click the following link to reset your GuildTools password: 

{url}

Thanks!";


            newEmail.HtmlContent =
                $@"Hello from the GuildTools Team!<br/><br/>

<a href='{url}'>Click here</a> to reset your GuildTools password.<br/><br/>

Thanks!";

            return newEmail;
        }

        public class ResetPasswordEmail
        {
            public string Subject { get; set; }
            public string TextContent { get; set; }
            public string HtmlContent { get; set; }
        }
    }
}
