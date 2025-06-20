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
    /// <summary>
    /// HTTP message handler for sending requests to APIM with Power Fx-specific headers and authentication.
    /// </summary>
    internal class ApimHttpHandler : HttpMessageHandler
    {
        /// <summary>
        /// Gets or sets the APIM endpoint.
        /// </summary>
        public string Endpoint { get; init; }

        /// <summary>
        /// Gets or sets the environment ID for the request.
        /// </summary>
        public string EnvironmentId { get; init; }

        /// <summary>
        /// Gets or sets the connection ID for the request.
        /// </summary>
        public string ConnectionId { get; init; }

        /// <summary>
        /// Gets or sets the function to retrieve the authentication token.
        /// </summary>
        public Func<Task<string>> GetAuthToken { get; init; }

        /// <summary>
        /// Gets or sets the HTTP message invoker used to send the request.
        /// </summary>
        public HttpMessageInvoker _invoker { get; init; }

        /// <summary>
        /// Session Id for telemetry.
        /// </summary>
        public string SessionId { get; set; } = Guid.NewGuid().ToString(); // "f4d37a97-f1c7-4c8c-80a6-f300c651568d"

        /// <summary>
        /// Gets the user agent string for telemetry.
        /// </summary>
        public string UserAgent { get; }

        /// <summary>
        /// Gets the version of the PowerPlatformConnectorClient assembly for telemetry.
        /// </summary>
        public static string Version => typeof(PowerPlatformConnectorClient).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion.Split('+')[0];

        /// <summary>
        /// Initializes a new instance of the <see cref="ApimHttpHandler"/> class.
        /// </summary>
        public ApimHttpHandler()
        {
            string userAgent = null;
            this.UserAgent = string.IsNullOrWhiteSpace(userAgent) ? $"PowerFx/{Version}" : $"{userAgent} PowerFx/{Version}";
        }

        /// <summary>
        /// Sends an HTTP request asynchronously with APIM-specific headers and authentication.
        /// </summary>
        /// <param name="request">The HTTP request message to send.</param>
        /// <param name="cancellationToken">A cancellation token to cancel operation.</param>
        /// <returns>The HTTP response message.</returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            using HttpRequestMessage req = await Transform(request).ConfigureAwait(false);
            return await _invoker.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Transforms the incoming request into a new request with APIM-specific headers and authentication.
        /// </summary>
        /// <param name="request">The original HTTP request message.</param>
        /// <returns>A new <see cref="HttpRequestMessage"/> with the required headers and authentication.</returns>
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
                    /* 
                       Client has Basepath set.
                       x-ms-request-url needs relative URL.
                    */

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

            /*
                might be needed for tabular connectors
                req.Headers.Add("X-Ms-Protocol-Semantics", "cdp");
            */

            foreach (var header in request.Headers)
            {
                req.Headers.Add(header.Key, header.Value);
            }

            req.Content = request.Content;

            return req;
        }
    }
}
