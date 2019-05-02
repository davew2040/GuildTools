using Microsoft.AspNetCore.Http;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace GuildTools.ErrorHandling
{
    public class CustomExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public CustomExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                if (!(ex is UserReportableError))
                { 
                    Log.Error($"Something went wrong: {ex}");
                }

                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            if (exception is ApiError error)
            {
                context.Response.StatusCode = error.StatusCode;
                return context.Response.WriteAsync(error.GetErrorDetails().ToString());
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            return context.Response.WriteAsync(
                new ApiErrorDetails(
                    "Internal Server Error",
                    context.Response.StatusCode,
                    ApiError.DefaultErrorType)
                .ToString());
        }
    }
}
