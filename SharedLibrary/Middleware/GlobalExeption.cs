using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Logs;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Net;
using System.Text;
using System.Text.Json;

namespace SharedLibrary.Middleware
{
    public class GlobalExeption(RequestDelegate next)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            //default
            string message = "Internal server error";
            int statusCode = (int)HttpStatusCode.InternalServerError;
            string title = "Error";

            try
            {
                await next(context);
                //check if exception is too many request
                if(context.Response.StatusCode == StatusCodes.Status429TooManyRequests)
                {
                    title = "Warning";
                    message = "Too many request made.";
                    statusCode = (int)StatusCodes.Status429TooManyRequests;
                    await ModifyHeader(context, title, message, statusCode);
                }
                //check if Response is Unathorized
                if (context.Response.StatusCode == StatusCodes.Status401Unauthorized)
                {
                    title = "Alert";
                    message = "You are not authorized to access";
                    await ModifyHeader(context, title, message, statusCode);
                }
                //check if respond is forbiden
                if (context.Response.StatusCode == StatusCodes.Status403Forbidden)
                {
                    title = "Out of Access";
                    message = "You are not allowed to access";
                    statusCode = StatusCodes.Status403Forbidden;
                    await ModifyHeader(context, title, message, statusCode);
                }

            }catch(Exception ex)
            {
                //Log Exceptions
                LogExeption.LogExeptions(ex);

                //check if exception is timeout
                if(ex is TaskCanceledException || ex is TimeoutException)
                {
                    title = " Out of time";
                    message = "Request timeout...try again";
                    statusCode = StatusCodes.Status408RequestTimeout;
                }
                //if one of then default
                await ModifyHeader(context, title, message, statusCode);
            }
        }

        private static async Task ModifyHeader(HttpContext context, string title, string message, int statusCode)
        {
            //Display message to the client
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new ProblemDetails({
                Detail = message,
                Status = statusCode,
                Title = title
            }), CancellationToken.None);
            return;
        }
    }
}
