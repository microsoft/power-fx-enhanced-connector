// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.PowerFx.Connectors;

#pragma warning disable CA2000 // Dispose objects before losing scope
#pragma warning disable CA1303 // Do not pass literals as localized parameters

namespace CdpValidator
{
    /// <summary>
    /// Entry point for the CDP Validator application.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Main entry point. Parses arguments, sets up the program, and runs validation.
        /// </summary>
        /// <param name="args">Command-line arguments.</param>
        public static void Main(string[] args)
        {            
            Args arguments = Args.Parse(args);

            if (arguments == null)
            {
                return;
            }

            Console.WriteLine($"Validate CDP");

            Console.WriteLine($"  AuthPath = {arguments.AuthPath}");
            Console.WriteLine($"  Mode = {arguments.Mode}");
            Console.WriteLine($"  LogDir = {arguments.LogDir}");

            Program program = new Program(arguments);

            // Allow 15 minutes for the validation to execute
            using CancellationTokenSource cts = new CancellationTokenSource(new TimeSpan(0, 15, 0));

            try
            {
                program.WorkAsync(cts.Token).Wait();
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.ToString());
                Console.ResetColor();
            }
            finally
            {
                arguments.HttpClient?.Dispose();
            }
        }

        private readonly Args _args;

        /// <summary>
        /// Initializes a new instance of the <see cref="Program"/> class.
        /// </summary>
        /// <param name="args">Parsed command-line arguments.</param>
        public Program(Args args)
        {
            _args = args;            
        }

        /// <summary>
        /// Runs the validation worker asynchronously.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        public async Task WorkAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();                                               
            await new ValidationWorker(_args).Validate(cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Logger implementation that writes connector logs to the console.
    /// </summary>
    internal class ConsoleLogger : ConnectorLogger
    {
        /// <summary>
        /// Gets the singleton instance of the <see cref="ConsoleLogger"/>.
        /// </summary>
        public static readonly ConnectorLogger Instance = new ConsoleLogger();

        /// <summary>
        /// Logs a connector log entry to the console.
        /// </summary>
        /// <param name="log">The connector log entry.</param>
        protected override void Log(ConnectorLog log)
        {
            // Console.Write(log.Message);
            //Console.Write(log.Exception);
        }
    }
}
