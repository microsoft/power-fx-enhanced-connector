// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace CdpValidator
{
    /*
     Add logging to a Delegating handler.
     This can be used with HttpClient like so:
     new HttpClient(new LoggingHttpMessageHandler(_requestResponseLogger))
    */

    /// <summary>
    /// A delegating handler that logs HTTP requests and responses for diagnostic purposes.
    /// </summary>
    internal class LoggingHttpMessageHandler : DelegatingHandler
    {
        private static readonly HttpClientHandler _default = new HttpClientHandler();
        private readonly RequestResponseLogger _logger;

        /// <summary>
        /// Stores the log of HTTP requests.
        /// </summary>
        internal List<RequestLog> _requestLog = new List<RequestLog>();    
        internal object _lock = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggingHttpMessageHandler"/> class.
        /// </summary>
        /// <param name="logger">The logger used to log requests and responses.</param>
        /// <param name="inner">The inner HTTP message handler to send requests to.</param>
        public LoggingHttpMessageHandler(RequestResponseLogger logger, HttpMessageHandler inner = null)
            : base(inner ?? _default)
        {
            _logger = logger;                        
        }

        /// <summary>
        /// Clears the request log.
        /// </summary>
        internal void ResetLog()
        {
            _requestLog.Clear();
        }

        /// <summary>
        /// Sends an HTTP request asynchronously and logs the request and response.
        /// </summary>
        /// <param name="request">The HTTP request message to send.</param>
        /// <param name="cancellationToken">A cancellation token to cancel operation.</param>
        /// <returns>The HTTP response message.</returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            /*
                By now, outer HttpClient has run and Endpoint has been resolved. 
                We should have an absolute Uri. 

                We can invoke the inner handler from here. 
                If we tried to call a HttpMessageInvoker, we'd fail because the request is already
                in-process of being sent. 
            */

            cancellationToken.ThrowIfCancellationRequested();
            RequestLog rl = null;

            // Save query to generate .http file if needed (HttpRequestMessage is disposable)
            lock (_lock)
            {
                rl = new RequestLog()
                {
                    UtcTicks = DateTime.UtcNow.Ticks,
                    Method = request.Method,
                    Uri = request.RequestUri,
                    Headers = request.Headers.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.FirstOrDefault())
                };
                _requestLog.Add(rl);
            }

            // Network call
            var resp = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

            rl.StatusCode = resp.StatusCode;

            // Read result
            var content = await resp.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);            

            if (_logger != null)
            {
                await _logger.LogAsync(resp, request, cancellationToken).ConfigureAwait(false);
            }

            return resp;
        }
    }

    /// <summary>
    /// Represents a log entry for an HTTP request and response.
    /// </summary>
    internal class RequestLog
    {
        /// <summary>
        /// Gets or sets the UTC timestamp (in ticks) when the request was made.
        /// </summary>
        internal long UtcTicks;

        /// <summary>
        /// Gets or sets the URI of the request.
        /// </summary>
        internal Uri Uri;

        /// <summary>
        /// Gets or sets the HTTP method of the request.
        /// </summary>
        internal HttpMethod Method;

        /// <summary>
        /// Gets or sets the HTTP status code of the response.
        /// </summary>
        internal HttpStatusCode StatusCode;

        /// <summary>
        /// Gets or sets the headers of the request.
        /// </summary>
        internal Dictionary<string, string> Headers;
    }
}