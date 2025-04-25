// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using Microsoft.PowerFx.Connectors;

namespace E2eTests
{
    // Basic tests to call CDP rest endpoints directly (without PowerFx).
    public class BasicTests
        : IClassFixture<CustomWebApplicationFactory>
    {
        // Spins up in-memory server.
        // This will use an in-memory ServiceNow provider with mocked requests.
        private readonly CustomWebApplicationFactory _factory;

        private HttpClient _client;

        public BasicTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = _factory.CreateClientWithAuth();
        }

        // Basic call to directly to rest API.
        [Fact]
        public async Task DirectGetMetadata()
        {
            var result = await _client.GetFromJsonAsync<DatasetMetadata>("$metadata.json/datasets").ConfigureAwait(true);

            // Results are specific to the provider.
            // $$$ Validate more.
            Assert.Equal("site", result.Tabular.DisplayName);
        }

        [Fact]
        public async Task DirectGetAllTables()
        {
            string dataset = "powerfx_catalog.default.covid...default";
            string url = $"datasets/{dataset}/tables";

            // Results here are mocked via Responses\AllTables.json.
            GetTablesResponse result = await _client.GetFromJsonAsync<GetTablesResponse>(url).ConfigureAwait(true);

            var table = result.Value.First(item => item.Name == "powerfx_catalog.default.covid");
            Assert.Equal("powerfx_catalog.default.covid", table.Name);
            Assert.Equal("covid", table.DisplayName);

            Assert.Single(result.Value);
        }
    }
}
