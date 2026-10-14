// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;

namespace CdpSampleWebApi.Tests
{
    // Hosts the sample in memory.
    public sealed class SampleApiFactory : WebApplicationFactory<Program>
    {
        // Replaces the registered ITableProviderFactory when set.
        public ITableProviderFactory ProviderFactory { get; init; }

        // Extra configuration values, for example "TableProvider:Endpoint".
        public Dictionary<string, string> Settings { get; } = new Dictionary<string, string>();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((context, config) => config.AddInMemoryCollection(Settings));

            builder.ConfigureTestServices(services =>
            {
                if (ProviderFactory != null)
                {
                    services.AddSingleton(ProviderFactory);
                }
            });
        }
    }
}
