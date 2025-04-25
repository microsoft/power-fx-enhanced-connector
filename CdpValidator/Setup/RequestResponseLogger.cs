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

    public class LogRequestResponse
    {
        public LogRequest Request { get; set; }

        public LogResponse Response { get; set; }
    }

    public class LogRequest
    {
        public string Method { get; set; }

        public string Url { get; set; }

        // Headers?
    }

    public class LogResponse
    {
        public int StatusCode { get; set; }

        [YamlMember(ScalarStyle = ScalarStyle.Literal)]
        public string Body { get; set; }
    }

    /// <summary>
    /// Helpers to Log a HttpRequestMessage to a directory.
    /// </summary>
    public class RequestResponseLogger
    {
        private readonly string _outputDir;

        private readonly bool _logToConsole;

        private int _counter;

        public RequestResponseLogger(string outputDir, bool logUrisToConsole)
        {
            _outputDir = outputDir;
            _logToConsole = logUrisToConsole;

            if (_outputDir != null)
            {
                System.IO.Directory.CreateDirectory(outputDir);
            }
        }

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
                var body = await response.Content.ReadAsStringAsync(cancel).ConfigureAwait(false);

                // Pretty print the body... (also validate it's correctly formatted)
                if (response.Content?.Headers?.ContentType?.MediaType == "application/json")
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
