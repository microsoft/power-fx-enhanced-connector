// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CdpSampleWebApi
{
    /// <summary>
    /// Exception filter that handles unhandled exceptions during a request and returns a standard error response.
    /// </summary>
    /// <remarks>
    /// See: https://learn.microsoft.com/aspnet/core/mvc/controllers/filters#exception-filters .
    /// </remarks>
    public class ExceptionHandler : IExceptionFilter
    {
        /// <summary>
        /// Called when an exception occurs during request processing.
        /// Sets the result to a JSON error response with status code 500.
        /// </summary>
        /// <param name="context">The exception context.</param>
        public void OnException(ExceptionContext context)
        {
            var ex = context.Exception;

            var req = context.HttpContext.Request;

            var error = new ErrorPoco
            {
                Message = ex.Message,
                RequestUri = $"{req.Path}{req.QueryString}",
                Details = ex.ToString()
            };
            context.Result = new ContentResult
            {
                Content = JsonSerializer.Serialize(error),
                ContentType = "application/json",
                StatusCode = 500
            };
        }
    }

    /// <summary>
    /// Standard object to return on errors (mostly unhandled exceptions).
    /// </summary>
    public class ErrorPoco
    {
        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the request URI where the error occurred.
        /// </summary>
        public string RequestUri { get; set; }

        /// <summary>
        /// Gets or sets the detailed error information.
        /// </summary>
        public string Details { get; set; }
    }
}
