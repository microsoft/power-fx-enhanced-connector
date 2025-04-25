// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CdpSampleWebApi
{
    // Run on unhandled exception during a request.
    // https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/filters?view=aspnetcore-9.0#exception-filters
    public class ExceptionHandler : IExceptionFilter
    {
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

    // Standard object to return on errors (mostly unhandled exceptions).
    // $$$ Sync with CDP spec for error protocol?
    public class ErrorPoco
    {
        public string Message { get; set; }

        public string RequestUri { get; set; }

        public string Details { get; set; }
    }
}
