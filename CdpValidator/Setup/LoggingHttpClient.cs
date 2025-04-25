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
    // Add logging to a Delegating handler.
    // This can be used with HttpClient like so:
    //   new HttpClient(new LoggingHttpMessageHandler(_requestResponseLogger))
    internal class LoggingHttpMessageHandler : DelegatingHandler
    {
        private static readonly HttpClientHandler _default = new HttpClientHandler();
        private readonly RequestResponseLogger _logger;

        internal List<RequestLog> _requestLog = new List<RequestLog>();    
        internal object _lock = new object();

        public LoggingHttpMessageHandler(RequestResponseLogger logger, HttpMessageHandler inner = null)
            : base(inner ?? _default)
        {
            _logger = logger;                       
        }

        internal void ResetLog()
        {
            _requestLog.Clear();
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // By now, outer HttpClient has run and Endpoint has been resolved. 
            // We should have an absolute Uri. 

            // We can invoke the inner handler from here. 
            // If we tried to call a HttpMessageInvoker, we'd fail because the request is already
            // in-process of being sent. 

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

            // network call
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

    internal class RequestLog
    {
        internal long UtcTicks;
        internal Uri Uri;
        internal HttpMethod Method;
        internal HttpStatusCode StatusCode;
        internal Dictionary<string, string> Headers;
    }
}