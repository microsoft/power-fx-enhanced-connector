// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using static CdpValidator.Extensions;

#pragma warning disable CA1303 // CA1303: Do not pass literals as localized parameters

namespace CdpValidator
{
    // -auth <filename>  // used to resolve an HttpClient with auth
    // -log <dir>       // write all log files to the directory
    // -mode: stress    //

    // -repl         // Run Power Fx repl. For interactive debugging
    [SuppressMessage("Design", "CA1001: Types that own disposable fields should be disposable", Justification = "Managed internally")]    
    public class Args
    {
        public string AuthPath { get; set; }

        public RunMode Mode { get; set; }

        public string LogDir { get; set; }

        private HttpClient _httpClient;
        private RequestResponseLogger _responseLogger;
        private CdpInternalConnection _internalConnection;
        private string _urlPrefix;
        internal LoggingHttpMessageHandler _httpHandler;

        public HttpClient HttpClient
        {
            get
            {
                _httpClient ??= GetHttpClient();
                return _httpClient;
            }
        }

        public string UrlPrefix
        {
            get
            {
                _urlPrefix ??= GetUrlPrefix();
                return _urlPrefix;
            }
        }

        private RequestResponseLogger ResponseLogger
        {
            get
            {
                _responseLogger ??= GetRequestResponseLogger();
                return _responseLogger;
            }
        }

        public CdpInternalConnection Connection
        {
            get
            {
                _internalConnection ??= GetInternalConnection();
                return _internalConnection;
            }
        }

        [SuppressMessage("Reliability", "CA2000: Dispose objects before losing scope", Justification = "Caller need to dispose object")]
        private HttpClient GetHttpClient()
        {
            _httpHandler = new LoggingHttpMessageHandler(ResponseLogger);
            var httpClient = new HttpClient(_httpHandler);

            Console.WriteLine($"Creating http client to: {Connection.endpoint}");

            if (string.IsNullOrEmpty(Connection.environmentId))
            {
                Console.WriteLine("No EnvironmentId, using direct connection");

                var endpoint = Connection.endpoint;

                if (!endpoint.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && !endpoint.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException($"Invalid endpoint: {endpoint}. Must start with http:// or https://");
                }

                httpClient.BaseAddress = new Uri(endpoint);

                if (!string.IsNullOrEmpty(Connection.jwtFile))
                {
                    Console.WriteLine("JWT file provided, using authentication");

                    string accessToken = File.ReadAllText(Connection.jwtFile);
                    if (!accessToken.StartsWith("bearer ", StringComparison.OrdinalIgnoreCase))
                    {
                        accessToken = "Bearer " + accessToken;
                    }

                    httpClient.DefaultRequestHeaders.Add("authorization", accessToken);
                }

                return httpClient;
            }
            else
            {
                Console.WriteLine($"EnvironmentId {Connection.environmentId}, using APIM");

                string jwt = File.ReadAllText(Connection.jwtFile);
                HttpClient client = CreateApimHttpClient(Connection.endpoint, Connection.environmentId, Connection.connectionId, () => Task.FromResult(jwt), httpClient);

                return client;
            }
        }

        private CdpInternalConnection GetInternalConnection()
        {
            Console.WriteLine($"Reading connection from file {AuthPath}");
            string json = File.ReadAllText(AuthPath);

            CdpInternalConnection connection = JsonSerializer.Deserialize<CdpInternalConnection>(json);

            Console.WriteLine($"  ConnectionId = {Display(connection.connectionId)}");
            Console.WriteLine($"  dataset = {Display(connection.dataset)}");
            Console.WriteLine($"  Endpoint = {Display(connection.endpoint)}");
            Console.WriteLine($"  EnvironmentId = {Display(connection.environmentId)}");
            Console.WriteLine($"  JwtFile = {Display(connection.jwtFile)}");
            Console.WriteLine($"  Tablename = {Display(connection.tablename)}");
            Console.WriteLine($"  Urlprefix = {Display(connection.urlprefix)}");            
            
            return connection;
        }

        // $$$ Can this be part of httpClient?
        // Beware: https://stackoverflow.com/a/23438417/534514
        private string GetUrlPrefix()
        {            
            string urlPrefix = Connection.urlprefix ?? string.Empty;
            urlPrefix = urlPrefix.Replace("{connectionId}", Connection.connectionId, StringComparison.OrdinalIgnoreCase);
            return urlPrefix;
        }

        [SuppressMessage("Reliability", "CA2000: Dispose objects before losing scope", Justification = "Caller is responsible to dispose the return value")]
        private static HttpClient CreateApimHttpClient(string endpoint, string environmentId, string connectionId, Func<Task<string>> getAuthToken, HttpMessageInvoker httpInvoker)
        {
            var handler = new ApimHttpHandler()
            {
                Endpoint = endpoint,
                EnvironmentId = environmentId,
                ConnectionId = connectionId,
                GetAuthToken = getAuthToken,
                _invoker = httpInvoker
            };

            // BaseAddress will be ignored because we will ultimately send an entirely new Request object to the invoker
            HttpClient client = new HttpClient(handler)
            {
                BaseAddress = new Uri($"https://{endpoint}")
            };

            return client;
        }

        private RequestResponseLogger GetRequestResponseLogger()
        {
            // $$$ Still log to console?
            var logDir = this.LogDir;
            if (logDir == null)
            {
                //return null;
            }

            return new RequestResponseLogger(logDir, logUrisToConsole: true);
        }

        public static Args Parse(string[] args)
        {
            var result = new Args();

            if (args.Length == 0)
            {
                Console.WriteLine("CdpValidator.exe [-auth <file>] [-mode { RunStress | Repl }] [-logdir <folder>]");
                Console.WriteLine();
                Console.WriteLine("  <file>    : Json file containing the connection parameters");
                Console.WriteLine("              Connection parameters: endpoint, environmentId, connectionId, urlprefix");
                Console.WriteLine("                                   : dataset, tablename, jwtFile");
                Console.WriteLine("  mode      : RunStress for validation");
                Console.WriteLine("            : Repl for an interactive session");
                Console.WriteLine("  <folder>  : Folder for detailed logs (network traces)");
                Console.WriteLine();
                return null;
            }

            int i = 0;

            while (i < args.Length)
            {
                string arg = args[i].ToLowerInvariant();
                i++;

                string value = args[i];
                i++;

                if (arg == "-auth")
                {
                    result.AuthPath = value;
                    continue;
                }
                else if (arg == "-mode")
                {
                    result.Mode = Enum.Parse<RunMode>(value, ignoreCase: true);
                    continue;
                }
                else if (arg == "-logdir")
                {
                    result.LogDir = value;
                }
                else
                {
                    throw new InvalidOperationException($"Unrecognized arg: {arg}");
                }
            }

            return result;
        }
    }

    public enum RunMode
    {
        RunStress,

        Repl,
    }
}
