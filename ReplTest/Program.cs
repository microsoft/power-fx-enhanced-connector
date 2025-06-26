// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.PowerFx;
using Microsoft.PowerFx.Connectors;
using Microsoft.PowerFx.Dataverse;
using Microsoft.PowerFx.Types;

namespace ReplTest
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            // Enable HTTP tracing before any HttpClient is created
            SubscribeToHttpDiagnostics();

            Console.WriteLine("Power Fx Repl for Tabular Connectors!");

            WorkAsync().Wait();
        }

        // Replace the lambda expression with an explicit implementation of IObserver<DiagnosticListener>
        private static void SubscribeToHttpDiagnostics()
        {
            DiagnosticListener.AllListeners.Subscribe(new DiagnosticListenerObserver());
        }

        private class DiagnosticListenerObserver : IObserver<DiagnosticListener>
        {
            public void OnNext(DiagnosticListener listener)
            {
                if (listener.Name == "HttpHandlerDiagnosticListener")
                {
                    listener.Subscribe(new HttpDiagnosticsObserver());
                }
            }

            public void OnError(Exception error)
            {
                // Handle errors if necessary
            }

            public void OnCompleted()
            {
                // Handle completion if necessary
            }
        }

        private class HttpDiagnosticsObserver : IObserver<KeyValuePair<string, object>>
        {
            public void OnNext(KeyValuePair<string, object> value)
            {
                if (value.Key == "System.Net.Http.HttpRequestOut.Start")
                {
                    var request = (HttpRequestMessage)value.Value.GetType().GetProperty("Request")?.GetValue(value.Value);
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine($"[HTTP TRACE] Request: {request?.Method} {request?.RequestUri}");
                    Console.ResetColor();
                }
                else if (value.Key == "System.Net.Http.HttpRequestOut.Stop")
                {
                    var response = value.Value.GetType().GetProperty("Response")?.GetValue(value.Value) as HttpResponseMessage;
                    if (response != null)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.WriteLine($"[HTTP TRACE] Response: {(int)response.StatusCode} {response.ReasonPhrase} for {response.RequestMessage.RequestUri}");
                        Console.ResetColor();
                    }
                }
            }

            public void OnError(Exception error) 
            { 
            }

            public void OnCompleted() 
            { 
            }
        }

        public static async Task WorkAsync()
        {
            // Add tabular connector to the repl
            string uri = "https://localhost:7157";
            string dataset = "default";

            HttpClient client = new HttpClient
            {
                BaseAddress = new Uri(uri)
            };
            
            // Get tables. 
            CdpDataSource cds = new CdpDataSource(dataset);
            string prefix = string.Empty;
            var tables = await cds.GetTablesAsync(client, prefix, default);

            var symbolValues = new SymbolValues("Connectors");

            Console.WriteLine($"Adding tables from '{dataset}' at {uri}:");
            foreach (var connectorTable in tables)
            {
                string name = connectorTable.TableName;
                Console.WriteLine($"  {name}");

                await connectorTable.InitAsync(client, prefix, default);

                TableValue tableValue = connectorTable.GetTableValue();

                symbolValues.Add(name, tableValue);
            }

            MyContext ctx = new MyContext
            {
                _client = client
            };

            var runtimeServices = new BasicServiceProvider();

            var engine = new RecalcEngine();

            runtimeServices.AddRuntimeContext(ctx);

            var repl = new PowerFxREPL();
            repl.Engine = engine;
            repl.Engine.EnableDelegation();
            repl.ExtraSymbolValues = symbolValues;
            repl.InnerServices = runtimeServices;

            // Run the repl. 
            while (!repl.ExitRequested)
            {
                await repl.WritePromptAsync();

                var line = Console.ReadLine();

                await repl.HandleLineAsync(line);
            }
        }

        private class MyContext : BaseRuntimeConnectorContext
        {
            public override TimeZoneInfo TimeZoneInfo => TimeZoneInfo.Local;

            public HttpClient _client;

            public override HttpMessageInvoker GetInvoker(string @namespace)
            {
                return _client;
            }
        }
    }
}
