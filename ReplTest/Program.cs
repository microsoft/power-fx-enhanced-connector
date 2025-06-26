// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.PowerFx;
using Microsoft.PowerFx.Connectors;
using Microsoft.PowerFx.Types;

namespace ReplTest
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Power Fx Repl for Tabular Connectors!");

            WorkAsync().Wait();
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
            repl.ExtraSymbolValues = symbolValues;
            repl.InnerServices = runtimeServices;

            // Run the repl. 
            while (true)
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
