// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using Microsoft.PowerFx.Connectors;
using Microsoft.PowerFx.Types;

namespace E2eTests
{
    // Call through Power Fx infrastructure.
    // Power Fx provides much more aggressive testing - it will consume and validate many properties.
    public class PowerFxTests
        : IClassFixture<CustomWebApplicationFactory>
    {
        // Spins up in-memory server.
        // This will use an in-memory ServiceNow provider with mocked requests.
        private readonly CustomWebApplicationFactory _factory;

        private HttpClient _client;

        public PowerFxTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClientWithAuth();
        }

        // Actually call through Power Fx infrastructure.
        [Fact]
        public async Task TestViaPowerFx()
        {
            string dataSet = "default.default...default";
            CdpDataSource cdpDataSource = new CdpDataSource(dataSet);

            string tableName = "powerfx_catalog.default.covid";
            string prefix = string.Empty;

            bool useLogical = true;
            CdpTable cdpTable = await cdpDataSource.GetTableAsync(_client, prefix, tableName, useLogical, default).ConfigureAwait(true);

            await cdpTable.InitAsync(_client, prefix, default).ConfigureAwait(true);

            RecordType type = cdpTable.TableType.ToRecord();

            // validate basic fields and types .
            var fields = type.GetFieldTypes().OrderBy(x => x.DisplayName.Value ?? x.Name.Value).ToArray();
            var fieldStr = string.Join(",", fields.Take(5)
                .Select(x => $"{x.Name}:{x.Type}"));

            Assert.Equal("Data_Cases:Decimal,Data_Deaths:Decimal,Data_Population:Decimal,Data_Rate:Decimal,Date_Day:Decimal", fieldStr);

            Assert.Equal(10, fields.Length);

            // Validate delegation info.
            var ok = type.TryGetCapabilities(out var capabilities);
            Assert.True(ok);
            Assert.NotNull(capabilities);

            // $$$ validate more.
        }

        // $$$ Add expression tests that show some delegation.
        // First, Filter, Sort,
    }
}
