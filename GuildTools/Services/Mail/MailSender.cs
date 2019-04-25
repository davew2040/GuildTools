using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuildTools.Services.Mail
{
    public class MailSender : IMailSender
    {
        private string apiKey;

        public MailSender(string apiKey)
        {
            this.apiKey = apiKey;
        }

        public async Task<bool> SendMailAsync(string to, string from, string subject, string textContent, string htmlContent)
        {
            var client = new SendGridClient(apiKey);
            var msg = new SendGridMessage()
            {
                From = new EmailAddress("dwinterm@gmail.com", "GuildTools Team"),
                Subject = subject,
                PlainTextContent = textContent,
                HtmlContent = htmlContent   
            };
            msg.AddTo(new EmailAddress(to));

            var response = await client.SendEmailAsync(msg);

            return response.StatusCode == System.Net.HttpStatusCode.Accepted;
        }
    }
}
