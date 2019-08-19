using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace GuildTools.ErrorHandling
{
    public static class ExceptionMiddlewareExtensions
    {
        public static void ConfigureExceptionHandler(this IApplicationBuilder app)
        {
            app.UseExceptionHandler(appError =>
            {
                appError.Run(async context =>
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    context.Response.ContentType = "application/json";

                    var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                    if (contextFeature != null)
                    {
                        if (!(contextFeature.Error is UserReportableError))
                        {
                            string data = string.Empty;

                            if (contextFeature.Error.Data != null)
                            {
                                data = "Data: " + contextFeature.Error.Data.ToString();
                            }

                            Log.Logger.Error($"Something went wrong: {contextFeature.Error} {data}");
                        }

                        if (contextFeature.Error is ApiError)
                        {
                            var apiError = contextFeature.Error as ApiError;
                            await context.Response.WriteAsync(apiError.GetErrorDetails().ToString());
                        }
                    }
                });
            });
        }

        public static void ConfigureCustomExceptionMiddleware(this IApplicationBuilder app)
        {
            app.UseMiddleware<CustomExceptionMiddleware>();
        }
    }
}
