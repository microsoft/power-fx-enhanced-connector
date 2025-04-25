// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.PowerFx.Connectors;

#pragma warning disable CA2000 // Dispose objects before losing scope
#pragma warning disable CA1303 // Do not pass literals as localized parameters
#pragma warning disable SA1300, IDE1006 // Elements should begin with an upper case

namespace CdpValidator
{
    // $$$ - move this into Fx.Connectors.

    // Calls to APIM are via a relative URL.
    // HttpClient will coerce to an absolute URL and then forward to the HttpMessageHandler.
    // But our outgoing call to APIM needs to know the relative URL .
    // invoke

    internal class ApimHttpHandler : HttpMessageHandler
    {
        public string Endpoint { get; init; }

        public string EnvironmentId { get; init; }

        public string ConnectionId { get; init; }

        public Func<Task<string>> GetAuthToken { get; init; }

        public HttpMessageInvoker _invoker { get; init; }

        /// <summary>
        /// Session Id for telemetry.
        /// </summary>
        public string SessionId { get; set; } = Guid.NewGuid().ToString(); // "f4d37a97-f1c7-4c8c-80a6-f300c651568d"

        public string UserAgent { get; }

        /// <summary>
        /// For telemetry - assembly version stamp.
        /// </summary>
        public static string Version => typeof(PowerPlatformConnectorClient).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion.Split('+')[0];

        public ApimHttpHandler()
        {
            string userAgent = null;
            this.UserAgent = string.IsNullOrWhiteSpace(userAgent) ? $"PowerFx/{Version}" : $"{userAgent} PowerFx/{Version}";
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            using HttpRequestMessage req = await Transform(request).ConfigureAwait(false);
            return await _invoker.SendAsync(req, cancellationToken).ConfigureAwait(false);
        }

        // This will create a new request and send it. So the inner invoke can be another httpclient.
        public async Task<HttpRequestMessage> Transform(HttpRequestMessage request)
        {
            var url = request.RequestUri.OriginalString;
            if (request.RequestUri.IsAbsoluteUri)
            {
                string prefix = $"https://{this.Endpoint}";
                if (url.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    url = url.Substring(prefix.Length);
                }
                else
                {
                    // Client has Basepath set.
                    // x-ms-request-url needs relative URL.
                    throw new InvalidOperationException($"URL should be relative for x-ms-request-url property");
                }
            }

            url = url.Replace("{connectionId}", ConnectionId, StringComparison.OrdinalIgnoreCase);

            var method = request.Method;
            var authToken = await GetAuthToken().ConfigureAwait(false);

            var req = new HttpRequestMessage(HttpMethod.Post, $"https://{Endpoint}/invoke");
            req.Headers.Add("authority", Endpoint);
            req.Headers.Add("scheme", "https");
            req.Headers.Add("path", "/invoke");
            req.Headers.Add("x-ms-client-session-id", SessionId);
            req.Headers.Add("x-ms-request-method", method.ToString().ToUpperInvariant());
            req.Headers.Add("Authorization", "Bearer " + authToken);
            req.Headers.Add("x-ms-client-environment-id", "/providers/Microsoft.PowerApps/environments/" + EnvironmentId);
            req.Headers.Add("x-ms-user-agent", UserAgent);
            req.Headers.Add("x-ms-request-url", url);

            // might be needed for tabular connectors
            //req.Headers.Add("X-Ms-Protocol-Semantics", "cdp");

            foreach (var header in request.Headers)
            {
                req.Headers.Add(header.Key, header.Value);
            }

            req.Content = request.Content;

            return req;
        }
    }
}
