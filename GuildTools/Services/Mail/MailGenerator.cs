using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuildTools.Services.Mail
{
    public class MailGenerator: IMailGenerator
    {
        private readonly string _baseUrl;

        public MailGenerator(IHttpContextAccessor httpContext)
        {
            _baseUrl = this.GetBaseUrl(httpContext.HttpContext.Request);
        }

        public RegistrationConfirmationEmail GenerateRegistrationConfirmationEmail(string url)
        {
            var newEmail = new RegistrationConfirmationEmail();

            newEmail.Subject = "Registration Confirmation";

            newEmail.TextContent =
                $@"Hello from the GuildTools Team!

Click the following link to confirm your GuildTools account registration:

{url}

Thanks!";


            newEmail.HtmlContent =
                $@"
Hello from the GuildTools Team!

Click the following link to confirm your GuildTools account registration:<br/>
<br/>
<a href='{url}'>{url}<br/>
<br/>

Thanks!";

            return newEmail;
        }

        public ResetPasswordEmail GenerateResetPasswordEmail(string url)
        {
            var newEmail = new ResetPasswordEmail();

            newEmail.Subject = "Reset your GuildTools password:";

            newEmail.TextContent =
                $@"
Hello from the GuildTools Team!

Click the following link to reset your GuildTools password: 

{url}

Thanks!";


            newEmail.HtmlContent =
                $@"
Hello from the GuildTools Team!<br/><br/>

Click the following link to reset your GuildTools password: 

<a href='{url}'>{url}<br/><br/>

Thanks!";

            return newEmail;
        }

        // Note: this one requires the tail URL!
        public StatsGenerationComplete GenerateStatsCompleteEmail(string url)
        {
            var fullUrl = _baseUrl + url;

            var newEmail = new StatsGenerationComplete();

            newEmail.Subject = "Stats generation complete!";

            newEmail.TextContent =
                $@"
Hello from the GuildTools Team!

Your guild stats generation is complete: 

{fullUrl}

Have a great day!";


            newEmail.HtmlContent =
                $@"
Hello from the GuildTools Team!<br/><br/>

Your guild stats generation is complete: 

<a href='{fullUrl}'>{fullUrl}.<br/><br/>

Thanks!";

            return newEmail;
        }

        protected string GetBaseUrl(HttpRequest request)
        {
            string baseUrl = $"{request.Scheme}://{request.Host}{request.PathBase}";

            return baseUrl;
        }
    }
}
