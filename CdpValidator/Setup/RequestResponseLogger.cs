// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace CdpValidator
{
#pragma warning disable CA1849 // Call async methods when in an async method

    /// <summary>
    /// Represents a log entry containing both the HTTP request and response.
    /// </summary>
    public class LogRequestResponse
    {
        /// <summary>
        /// Gets or sets the logged HTTP request.
        /// </summary>
        public LogRequest Request { get; set; }

        /// <summary>
        /// Gets or sets the logged HTTP response.
        /// </summary>
        public LogResponse Response { get; set; }
    }

    /// <summary>
    /// Represents a logged HTTP request.
    /// </summary>
    public class LogRequest
    {
        /// <summary>
        /// Gets or sets the HTTP method.
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// Gets or sets the request URL.
        /// </summary>
        public string Url { get; set; }

        // Headers?
    }

    /// <summary>
    /// Represents a logged HTTP response.
    /// </summary>
    public class LogResponse
    {
        /// <summary>
        /// Gets or sets the HTTP status code.
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// Gets or sets the response body.
        /// </summary>
        [YamlMember(ScalarStyle = ScalarStyle.Literal)]
        public string Body { get; set; }
    }

    /// <summary>
    /// Helpers to log an HttpRequestMessage and HttpResponseMessage to a directory.
    /// </summary>
    public class RequestResponseLogger
    {
        private readonly string _outputDir;
        private readonly bool _logToConsole;
        private int _counter;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestResponseLogger"/> class.
        /// </summary>
        /// <param name="outputDir">The directory to write log files to.</param>
        /// <param name="logUrisToConsole">Whether to log URIs to the console.</param>
        public RequestResponseLogger(string outputDir, bool logUrisToConsole)
        {
            _outputDir = outputDir;
            _logToConsole = logUrisToConsole;

            if (_outputDir != null)
            {
                System.IO.Directory.CreateDirectory(outputDir);
            }
        }

        /// <summary>
        /// Generates a filename for a request based on its URI and method.
        /// </summary>
        /// <param name="req">The HTTP request message.</param>
        /// <returns>A filename string for the request log.</returns>
        [SuppressMessage("Design", "CA1055", Justification = "Pending - URI return values should not be strings")]
        public static string GetFilenameFromUri(HttpRequestMessage req)
        {
            var method = req.Method.ToString();

            var uri = req.RequestUri.ToString();

            // if req is full url, then hack.
            if (req.RequestUri.IsAbsoluteUri)
            {
                uri = req.RequestUri.PathAndQuery;
            }

            string hash = uri.GetHashCode(StringComparison.Ordinal).ToString("X", CultureInfo.InvariantCulture);

            // Hack off query string - to complex to express in a file.
            string querySuffix = string.Empty;
            int i = uri.IndexOf('?', StringComparison.Ordinal);
            if (i > 0)
            {
                string query = uri.Substring(i);
                uri = uri.Substring(0, i);

                querySuffix = $"-QUERY";
            }

            StringBuilder sb = new StringBuilder();

            sb.Append(method.ToUpperInvariant());

            var parts = uri.Split('/');
            foreach (var part in parts)
            {
                string value;
                if (part.IndexOfAny(new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' }) >= 0)
                {
                    value = "_";
                }
                else
                {
                    value = part.ToLowerInvariant();
                }

                sb.Append('-');
                sb.Append(value);
            }

            sb.Append(querySuffix);
            sb.Append('-');
            sb.Append(hash);
            sb.Append(".yaml");

            string filename = sb.ToString();
            return filename;
        }

        /// <summary>
        /// Logs the HTTP response (and optionally the request) to a file and/or the console.
        /// </summary>
        /// <param name="response">The HTTP response message.</param>
        /// <param name="request">The HTTP request message (optional).</param>
        /// <param name="cancel">A cancellation token.</param>
        public async Task LogAsync(HttpResponseMessage response, HttpRequestMessage request = null, CancellationToken cancel = default)
        {
            string countPrefix = $"{DateTime.UtcNow.ToString("O").Replace(':', '_')}_N{_counter:000}-";
            _counter++;

            // ex: '2025-03-12T09_00_12.1632340Z_N007-POST--invoke-395196E0.yaml'
            string outputPath = _outputDir != null ? Path.Combine(_outputDir, countPrefix + GetFilenameFromUri(request ?? response.RequestMessage)) : null;            

            if (_logToConsole)
            {
                var pathAndQuery = request.RequestUri.PathAndQuery;

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"{request.Method} {request.RequestUri} -> {(int)response.StatusCode}");                

                // APIM headers
                if (request.Headers.TryGetValues("x-ms-request-url", out IEnumerable<string> urlRequest) &&
                    request.Headers.TryGetValues("x-ms-request-method", out IEnumerable<string> method))
                {
                    Console.WriteLine($"  APIM: {method.First()} {urlRequest.First()}");
                }

                if (!string.IsNullOrEmpty(outputPath))
                {                 
                    Console.WriteLine($"  Log File: {outputPath}");
                }

                Console.ResetColor();
            }

            if (_outputDir != null)
            {               
                string body = await response.Content.ReadAsStringAsync(cancel).ConfigureAwait(false);                

                // Pretty print the body... (also validate it's correctly formatted)
                if (!body.Contains("\"error_code\":", StringComparison.OrdinalIgnoreCase) && 
                    response.Content?.Headers?.ContentType?.MediaType == "application/json")
                {                   
                    JsonElement json = JsonSerializer.Deserialize<JsonElement>(body);
                    body = JsonSerializer.Serialize(json, new JsonSerializerOptions { WriteIndented = true });                                               
                }

                var logReq = new LogRequest
                {
                    Method = request.Method.Method,
                    Url = request.RequestUri.ToString()
                };

                var logResp = new LogResponse
                {
                    StatusCode = (int)response.StatusCode,
                    Body = body
                };                                                               

                var log = new LogRequestResponse
                {
                    Request = logReq,
                    Response = logResp
                };

                var serializer = new SerializerBuilder().Build();
                var yaml = serializer.Serialize(log);

                File.WriteAllText(outputPath, yaml);
            }
        }
    }
}
