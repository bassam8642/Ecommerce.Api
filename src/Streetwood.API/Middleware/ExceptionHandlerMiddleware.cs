﻿using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using NLog;
using Streetwood.Core.Exceptions;

namespace Streetwood.API.Middleware
{
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate nextDelegate;
        private readonly ILogger logger;

        public ExceptionHandlerMiddleware(RequestDelegate nextDelegate, ILogger logger)
        {
            this.nextDelegate = nextDelegate;
            this.logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await nextDelegate(context);
            }
            catch (StreetwoodException exception)
            {
                logger.Warn($"Streetwood exception with code '{exception.ErrorCode.ToString()}.\n{exception.Message}");
                await HandleException(context, exception);
            }
            catch (Exception exception)
            {
                var message = PrepareExceptionMessage(exception);
                logger.Error(exception, message);
                await HandleException(context, exception);
            }
        }

        private Task HandleException(HttpContext context, Exception exception)
        {
            var statusCode = HttpStatusCode.InternalServerError;
            var errorCodeName = nameof(HttpStatusCode.InternalServerError);
            var message = exception.Message;

            if (exception is UnauthorizedAccessException)
            {
                errorCodeName = nameof(HttpStatusCode.Unauthorized);
                statusCode = HttpStatusCode.Unauthorized;
            }
            else if (exception is StreetwoodException streetwoodException)
            {
                statusCode = streetwoodException.ErrorCode.StatusCode;
                errorCodeName = streetwoodException.ErrorCode.ErrorCodeName;
                message = string.IsNullOrEmpty(message) ? streetwoodException.ErrorCode.Message : message;
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;
            var responseBody = JsonConvert.SerializeObject(new { errorCodeName, message });

            return context.Response.WriteAsync(responseBody);
        }

        private string PrepareExceptionMessage(Exception exception)
        {
            var message = exception.Message;
            while (exception.InnerException != null)
            {
                exception = exception.InnerException;
                message += $" --- Inner exception: {exception.Message}";
            }

            return message;
        }
    }
}
