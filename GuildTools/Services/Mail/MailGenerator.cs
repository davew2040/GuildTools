using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuildTools.Services.Mail
{
    public class MailGenerator
    {
        public static RegistrationConfirmationEmail GenerateRegistrationConfirmationEmail(string url)
        {
            var newEmail = new RegistrationConfirmationEmail();

            newEmail.Subject = "Registration Confirmation";

            newEmail.TextContent =
                $@"Hello from the GuildTools Team!

Click the following link to confirm your GuildTools account registration:

{url}

Thanks!";


            newEmail.HtmlContent =
                $@"Hello from the GuildTools Team!

Click the following link to confirm your GuildTools account registration:<br/>
<br/>
<a href='{url}'>{url}.<br/>
<br/>

Thanks!";

            return newEmail;
        }

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

Click the following link to reset your GuildTools password: 

<a href='{url}'>{url}.<br/><br/>

Thanks!";

            return newEmail;
        }

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
    }
}
