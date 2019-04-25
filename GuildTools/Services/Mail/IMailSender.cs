using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuildTools.Services.Mail
{
    public interface IMailSender
    {
        Task<bool> SendMailAsync(string to, string from, string subject, string textContent, string htmlContent);
    }
}
