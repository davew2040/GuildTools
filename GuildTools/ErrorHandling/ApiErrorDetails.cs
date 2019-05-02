using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuildTools.ErrorHandling
{
    public class ApiErrorDetails
    {
        public string Message { get; }
        public string ErrorType { get; }
        public int StatusCode { get; }

        public ApiErrorDetails(string message, int statusCode, string errorType)
        {
            this.Message = message;
            this.StatusCode = statusCode;
            this.ErrorType = errorType;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
