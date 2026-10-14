// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System.Net;

namespace CdpSampleWebApi.Tests
{
    public class CdpControllerTests
    {
        private const string Endpoint = "https://data.example.test";

        private static readonly Uri DatasetsUri = new Uri("datasets", UriKind.Relative);

        [Fact]
        public async Task GetDatasetsReturnsDefaultDataset()
        {
            using var factory = new SampleApiFactory();
            using var client = factory.CreateClient();

            using var response = await client.GetAsync(DatasetsUri);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            Assert.Contains("\"default\"", body, StringComparison.Ordinal);
        }

        [Fact]
        public async Task GetItemsReturnsRows()
        {
            using var factory = new SampleApiFactory();
            using var client = factory.CreateClient();

            using var response = await client.GetAsync(new Uri("datasets/default/tables/MyTable/items", UriKind.Relative));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            Assert.Contains("\"Ten\"", body, StringComparison.Ordinal);
        }

        [Fact]
        public async Task HealthReturnsOk()
        {
            using var factory = new SampleApiFactory();
            using var client = factory.CreateClient();

            using var response = await client.GetAsync(new Uri("$health", UriKind.Relative));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task ProviderReceivesSettingsFromConfiguration()
        {
            var providerFactory = new RecordingProviderFactory();
            using var factory = new SampleApiFactory { ProviderFactory = providerFactory };
            factory.Settings["TableProvider:Endpoint"] = Endpoint;
            using var client = factory.CreateClient();

            using var response = await client.GetAsync(DatasetsUri);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            AssertEndpointSetting(providerFactory.LastSettings);
        }

        [Fact]
        public async Task ProviderReceivesEmptySettingsWhenNoneAreConfigured()
        {
            var providerFactory = new RecordingProviderFactory();
            using var factory = new SampleApiFactory { ProviderFactory = providerFactory };
            using var client = factory.CreateClient();

            using var response = await client.GetAsync(DatasetsUri);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(providerFactory.LastSettings);
            Assert.Empty(providerFactory.LastSettings);
        }

        [Theory]
        [InlineData("e30=")]
        [InlineData("not-base64")]
        [InlineData("Bearer test-token")]
        public async Task AuthorizationHeaderDoesNotChangeProviderSettings(string value)
        {
            var providerFactory = new RecordingProviderFactory();
            using var factory = new SampleApiFactory { ProviderFactory = providerFactory };
            factory.Settings["TableProvider:Endpoint"] = Endpoint;
            using var client = factory.CreateClient();
            client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", value);

            using var response = await client.GetAsync(DatasetsUri);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            AssertEndpointSetting(providerFactory.LastSettings);
        }

        private static void AssertEndpointSetting(IReadOnlyDictionary<string, string> settings)
        {
            Assert.NotNull(settings);
            var setting = Assert.Single(settings);
            Assert.Equal("Endpoint", setting.Key);
            Assert.Equal(Endpoint, setting.Value);
        }

        private sealed class RecordingProviderFactory : ITableProviderFactory
        {
            public IReadOnlyDictionary<string, string> LastSettings { get; private set; }

            public ITableProvider Get(IReadOnlyDictionary<string, string> settings)
            {
                LastSettings = settings;
                return new TrivialTableProvider();
            }
        }
    }
}
