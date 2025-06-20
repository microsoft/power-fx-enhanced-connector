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
    /// <summary>
    /// Provides a REPL (Read-Eval-Print Loop) for debugging and interacting with Power Fx expressions and tables.
    /// </summary>
    internal class MyRepl
    {
        /// <summary>
        /// Gets the runtime service provider for the REPL.
        /// </summary>
        public readonly BasicServiceProvider _runtimeServices = new BasicServiceProvider();

        /// <summary>
        /// Gets the symbol values used in the REPL session.
        /// </summary>
        public readonly SymbolValues _symbolValues = new SymbolValues("Delegable_1");

        /// <summary>
        /// Adds a table to the symbol values for use in the REPL.
        /// </summary>
        /// <param name="name">The name of the table.</param>
        /// <param name="table">The table value to add.</param>
        public void AddTable(string name, TableValue table)
        {
            Console.WriteLine($"Added table '{name}'.");
            _symbolValues.Add(name, table);
        }

        /// <summary>
        /// Runs the REPL session asynchronously using the specified HTTP client connector.
        /// </summary>
        /// <param name="httpClientConnector">The HTTP client connector to use.</param>
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

    /// <summary>
    /// Provides a runtime connector context for the REPL, including HTTP client and time zone info.
    /// </summary>
    internal class MyContext : BaseRuntimeConnectorContext
    {
        /// <summary>
        /// Gets the local time zone info for the context.
        /// </summary>
        public override TimeZoneInfo TimeZoneInfo => TimeZoneInfo.Local;

        /// <summary>
        /// The HTTP client used for connector invocations.
        /// </summary>
        public HttpClient _client;

        /// <summary>
        /// Gets the HTTP message invoker for the specified namespace.
        /// </summary>
        /// <param name="namespace">The namespace for which to get the invoker.</param>
        /// <returns>The HTTP message invoker.</returns>
        public override HttpMessageInvoker GetInvoker(string @namespace)
        {
            return _client;
        }
    }
}
