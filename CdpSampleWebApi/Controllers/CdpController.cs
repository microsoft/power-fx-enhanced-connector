// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using CdpSampleWebApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.PowerFx.Connectors;
using Microsoft.PowerFx.Types;
using DatasetMetadataResponse = Microsoft.PowerFx.Connectors.DatasetMetadata; // Rename

namespace CdpSampleWebApi.Controllers
{
    /// <summary>
    /// Controller for handling Power Apps OData and table-related API requests.
    /// </summary>
    // For Power Apps, routes must be hosted directly at /, and not have any additional prefix. 
    // Webapp can have the extra prefix and things. It's just that the connector needs to have extra logic in Policies.xml
    // which is a runtime logic where we can update the backend url with prefixes. 
    // This sample doesn't authenticate callers. See readme.md before connecting it to real data.
    [ApiController]
    [Route("")]
    public class CdpController : ControllerBase
    {
        /// <summary>
        /// Configuration section whose values are passed to <see cref="ITableProviderFactory.Get"/>.
        /// </summary>
        public const string TableProviderSection = "TableProvider";

        private readonly ITableProviderFactory _providerFactory;

        private readonly IReadOnlyDictionary<string, string> _providerSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="CdpController"/> class.
        /// </summary>
        /// <param name="provider">The table provider factory.</param>
        /// <param name="configuration">The application configuration. Provider settings are read from the <c>TableProvider</c> section.</param>
        public CdpController(ITableProviderFactory provider, IConfiguration configuration)
        {
            _providerFactory = provider;
            _providerSettings = configuration.GetSection(TableProviderSection)
                .GetChildren()
                .Where(setting => setting.Value != null)
                .ToDictionary(setting => setting.Key, setting => setting.Value, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets dataset metadata for Power Apps.
        /// </summary>
        /// <returns>The dataset metadata response.</returns>
        [HttpGet]
        [Route("$metadata.json/datasets")]
        public async Task<DatasetMetadataResponse> GetMetadata()
        {
            var provider = GetProvider();
            
            return new DatasetMetadataResponse
            {
                Tabular = new MetadataTabular
                {
                    Source = "mru",
                    DisplayName = "site", // $$$
                    UrlEncoding = "double",
                    TableDisplayName = "DisplayName1",
                    TablePluralName = "DisplayNames1"
                }
            };
        }

        /// <summary>
        /// Gets the list of datasets.
        /// </summary>
        /// <returns>The dataset response.</returns>
        [HttpGet]
        [Route("datasets")]
        public async Task<DatasetResponse> GetDatasets()
        {
            var provider = GetProvider();

            var datasetItems = await provider.GetDatasetsAsync(CancellationToken.None).ConfigureAwait(true);

            return new DatasetResponse
            {
                value = datasetItems
            };
        }

        /// <summary>
        /// Gets the list of tables for a given dataset.
        /// </summary>
        /// <param name="dataset">The dataset name.</param>
        /// <returns>The tables response.</returns>
        [HttpGet]
        [Route("datasets/{dataset}/tables")]
        public async Task<GetTablesResponse> GetTables(string dataset)
        {           
            var provider = GetProvider();
            var result = await provider.GetTablesAsync(dataset, CancellationToken.None).ConfigureAwait(true);
            return result;
        }

        /// <summary>
        /// Gets the schema and capabilities for a specific table.
        /// </summary>
        /// <param name="dataset">The dataset name.</param>
        /// <param name="tableName">The table name.</param>
        /// <param name="apiVersion">The API version.</param>
        /// <returns>The table response.</returns>
        [HttpGet]
        [Route("$metadata.json/datasets/{dataset}/tables/{tableName}")]
        public async Task<GetTableResponse> GetTableInfo(
            string dataset,
            string tableName,
            [FromQuery(Name = "api-version")] string apiVersion)
        {
            var provider = GetProvider();
            RecordType record = await provider.GetTableAsync(dataset, tableName, CancellationToken.None).ConfigureAwait(true);

            GetTableResponse resp = record.ToTableResponse(tableName);

            return resp;
        }

        /// <summary>
        /// Gets the items (rows) for a specific table, applying OData query parameters.
        /// </summary>
        /// <param name="dataset">The dataset name.</param>
        /// <param name="tableName">The table name.</param>
        /// <param name="apiVersion">The API version.</param>
        /// <param name="odataParameters">The OData query parameters.</param>
        /// <returns>The items response.</returns>
        [HttpGet]
        [Route("datasets/{dataset}/tables/{tableName}/items")]
        public async Task<GetItemsResponse> GetItems(
            string dataset,
            string tableName,
            [FromQuery(Name = "api-version")] string apiVersion,
            [FromQuery] ODataQueryModel odataParameters)
        {            
            var provider = GetProvider();
            TableValue tableValue = await provider.GetTableValueAsync(dataset, tableName, CancellationToken.None).ConfigureAwait(true);
            
            var resp = await tableValue.ToGetItemsResponseAsync(odataParameters.ToStrDict(), CancellationToken.None).ConfigureAwait(true);

            return resp;
        }

        /// <summary>
        /// Gets the table provider for the current request.
        /// </summary>
        /// <remarks>
        /// Provider settings come from the <c>TableProvider</c> configuration section, not from the request.
        /// </remarks>
        /// <returns>The table provider.</returns>
        private ITableProvider GetProvider()
        {
            return _providerFactory.Get(_providerSettings);
        }
    }
}
