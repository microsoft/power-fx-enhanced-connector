// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System.Text;
using CdpSampleWebApi;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.PowerFx.Databricks;
using Microsoft.PowerFx.Databricks.Tests;
using Program = CdpSampleWebApi.Program;

namespace E2eTests
{
    // Spin up an HttpClient pointed at an in-memory server:
    // https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-10.0
    // Calling through in-memory server means e2e testing including:
    // - an httpClient that can be passed into Microsoft.PowerFx.Connector classes.
    // - routes
    // - model binding / json serialization.
    public class CustomWebApplicationFactory
    : WebApplicationFactory<Program>
    {
        public CustomWebApplicationFactory()
        {
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // Allow the test to override the services.
            // https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-9.0#customize-webapplicationfactory

            // This will get invoked by builder.Build(),
            // so it can overwrite production services set in Program.Main
            builder.ConfigureServices(services =>
            {
                // Create a SN client based on mock responses.
                HttpClient httpClient = new TestHttpClient();
                DatabricksClient dbClient = new DatabricksClient(httpClient);

                var provider = new DatabricksTableProvider(dbClient);
                var providerFactory = new TestProviderFactory
                {
                    Provider = provider
                };

                services.AddSingleton<ITableProviderFactory>(providerFactory);
            });

            base.ConfigureWebHost(builder);
        }

        private class TestProviderFactory : ITableProviderFactory
        {
            public DatabricksTableProvider Provider { get; set; }

            public ITableProvider Get(IReadOnlyDictionary<string, string> settings)
            {
                return this.Provider;
            }
        }

        public HttpClient CreateClientWithAuth()
        {
            var client = this.CreateClient();

            // Add an Authorization token.
            var json = "{ }";
            var bytes = Encoding.UTF8.GetBytes(json);
            var jwt = Convert.ToBase64String(bytes);

            // jwt can't have '=', parse error.
            client.DefaultRequestHeaders.Add("authorization", jwt);

            return client;
        }
    }
}
