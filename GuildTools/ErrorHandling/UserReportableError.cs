using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuildTools.ErrorHandling
{
    public class UserReportableError : ApiError
    {
        private const string UserErrorType = "Reportable";

        public UserReportableError(string message, int statusCode) : base(message, statusCode, UserErrorType)
        {

        }
    }
}
