// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.PowerFx;
using Microsoft.PowerFx.Connectors;
using Microsoft.PowerFx.Dataverse;
using Microsoft.PowerFx.Types;

namespace CdpValidator
{
#pragma warning disable CS0618 // Type or member is obsolete
    // Repl for debugging 
    internal class MyRepl
    {
        public readonly BasicServiceProvider _runtimeServices = new BasicServiceProvider();

        public readonly SymbolValues _symbolValues = new SymbolValues("Delegable_1");

        public void AddTable(string name, TableValue table)
        {
            Console.WriteLine($"Added table '{name}'.");
            _symbolValues.Add(name, table);
        }

        public async Task RunAsync(HttpClient httpClientConnector)
        {
            var config = new PowerFxConfig();
            config.EnableSetFunction();
            config.EnableJsonFunctions();
            var engine = new RecalcEngine();

            // Critical 
            engine.EnableDelegation();

            var repl = new PowerFxREPL
            {
                ExtraSymbolValues = _symbolValues,
                Engine = engine,
                AllowSetDefinitions = true
            };

            MyContext ctx = new MyContext
            {
                _client = httpClientConnector
            };
            _runtimeServices.AddRuntimeContext(ctx);

            repl.InnerServices = _runtimeServices;

            while (true)
            {
                await repl.WritePromptAsync()
                    .ConfigureAwait(false);

                var line = Console.ReadLine();

                // End of file
                if (line == null)
                {
                    return;
                }

                await repl.HandleLineAsync(line)
                    .ConfigureAwait(false);

                // Exit() function called
                if (repl.ExitRequested)
                {
                    return;
                }
            }
        }
    }

    internal class MyContext : BaseRuntimeConnectorContext
    {
        public override TimeZoneInfo TimeZoneInfo => TimeZoneInfo.Local;

        public HttpClient _client;

        public override HttpMessageInvoker GetInvoker(string @namespace)
        {
            return _client;
        }
    }
}
