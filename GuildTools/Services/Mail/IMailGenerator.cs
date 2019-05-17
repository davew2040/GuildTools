using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuildTools.Services.Mail
{
    public interface IMailGenerator
    {
        RegistrationConfirmationEmail GenerateRegistrationConfirmationEmail(string url);
        ResetPasswordEmail GenerateResetPasswordEmail(string url);
        StatsGenerationComplete GenerateStatsCompleteEmail(string url);
    }
}
