// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.PowerFx.Connectors
{
    /// <summary>
    /// Represents the entire CDP connection, including all datasets and tables.
    /// </summary>
    public class CdpRoot
    {
        private DatasetMetadata _metadata;
        private readonly HttpClient _httpClient;
        private readonly string _urlPrefix;
        private static readonly ConnectorLogger _logger = null;

        // Pass in default opts to ensure we really are enforcing casing.
        private JsonSerializerOptions _jsonOpts = new JsonSerializerOptions();

        /// <summary>
        /// Initializes a new instance of the <see cref="CdpRoot"/> class.
        /// </summary>
        /// <param name="httpClient">The HTTP client to use for requests.</param>
        /// <param name="urlPrefix">The URL prefix for the CDP API.</param>
        public CdpRoot(HttpClient httpClient, string urlPrefix)
        {
            _httpClient = httpClient;
            _urlPrefix = urlPrefix;
        }

        /// <summary>
        /// Gets the dataset metadata for the connection.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The dataset metadata.</returns>
        public async Task<DatasetMetadata> GetDatasetMetadataAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var datasetMetadata = await CdpDataSource.GetDatasetsMetadataAsync(_httpClient, _urlPrefix, cancellationToken, _logger).ConfigureAwait(false);

            _metadata = datasetMetadata;

            return datasetMetadata;
        }

        /// <summary>
        /// Enumerates all known datasets. If <see cref="DatasetMetadata.DatasetFormat"/> is set, there may be other datasets that can be constructed which do not show up in the enumeration.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A read-only collection of dataset items.</returns>
        public async Task<IReadOnlyCollection<DatasetResponse.Item>> GetDatasetsAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var list = new List<CdpDataSource>();
            var uri = _urlPrefix + "/datasets";

            using var req = new HttpRequestMessage(HttpMethod.Get, uri);

            var response = await _httpClient.GetFromJsonAsync<DatasetResponse>(uri, _jsonOpts, cancellationToken).ConfigureAwait(false);

            /*
                $$$ DatasetResponse.Item includes DisplayName,
                whereas CdpDatasource is just logical name. 
            */
            return response.value;
        }

        /// <summary>
        /// Gets the tables for a given data source.
        /// </summary>
        /// <param name="dataSource">The data source to get tables for.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>An enumerable of <see cref="CdpTable"/> objects.</returns>
        public async Task<IEnumerable<CdpTable>> GetTablesAsync(CdpDataSource dataSource, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            IEnumerable<CdpTable> tables = await dataSource.GetTablesAsync(_httpClient, _urlPrefix, cancellationToken).ConfigureAwait(false);

            /*
                $$$ Need this to handle errors properly.
                It throws PowerFxConnectorException, but add ErrorResponse to that.  
            */
            return tables;
        }
    }
}
