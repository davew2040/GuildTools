using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuildTools.ErrorHandling
{
    public class ApiError : Exception
    {
        public const string DefaultErrorType = "Server";

        public int StatusCode { get; }
        public string ErrorType { get; }

        public ApiError(string message, int statusCode): base(message)
        {
            this.StatusCode = statusCode;
            this.ErrorType = DefaultErrorType;
        }

        public ApiError(string message, int statusCode, string errorType) : base(message)
        {
            this.StatusCode = statusCode;
            this.ErrorType = errorType;
        }

        public ApiErrorDetails GetErrorDetails()
        {
            return new ApiErrorDetails(this.Message, this.StatusCode, this.ErrorType);
        }
    }
}
