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
    /// Represents Entire CDP connection - all datasets.  
    /// <see cref="CdpDataSource"/> - this is a dataset. 
    /// <see cref="CdpTable"/> - this is one table.
    /// </summary>
    public class CdpRoot
    {
        private DatasetMetadata _metadata;

        private readonly HttpClient _httpClient;
        private readonly string _urlPrefix;

        private static readonly ConnectorLogger _logger = null;

        // Pass in defaulkt opts  to ensure we reall are enforcing casing.
        private JsonSerializerOptions _jsonOpts = new JsonSerializerOptions();

        public CdpRoot(HttpClient httpClient, string urlPrefix)
        {
            _httpClient = httpClient;
            _urlPrefix = urlPrefix;
        }

        // URL = /$metadata.json/datasets
        public async Task<DatasetMetadata> GetDatasetMetadataAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var datasetMetadata = await CdpDataSource.GetDatasetsMetadataAsync(_httpClient, _urlPrefix, cancellationToken, _logger).ConfigureAwait(false);

            _metadata = datasetMetadata;

            return datasetMetadata;
        }

        /// <summary>
        /// Enumerate all known datasets.  
        /// However - if there's a <see cref="DatasetMetadata.DatasetFormat"/> set, then there may be 
        /// other dataset that can be constructed which don't show up in the enumeration.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<IReadOnlyCollection<DatasetResponse.Item>> GetDatasetsAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var list = new List<CdpDataSource>();
            var uri = _urlPrefix + "/datasets";

            using var req = new HttpRequestMessage(HttpMethod.Get, uri);

            var response = await _httpClient.GetFromJsonAsync<DatasetResponse>(uri, _jsonOpts, cancellationToken).ConfigureAwait(false);

            // $$$ DatasetResponse.Item includes DisplayName,
            // whereas CdpDatasource is just logical name. 
            return response.value;
        }

        public async Task<IEnumerable<CdpTable>> GetTablesAsync(CdpDataSource dataSource, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            IEnumerable<CdpTable> tables = await dataSource.GetTablesAsync(_httpClient, _urlPrefix, cancellationToken).ConfigureAwait(false);

            // $$$ Need this to handle errors properly.
            // It throws PowerFxConnectorException, but add ErrorResponse to that.  

            return tables;
        }
    }
}
