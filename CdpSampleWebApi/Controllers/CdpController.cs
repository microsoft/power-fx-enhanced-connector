// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System.Text.Json;
using CdpHelpers.Protocol;
using CdpSampleWebApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.PowerFx.Connectors;
using Microsoft.PowerFx.Types;
using DatasetMetadataResponse = Microsoft.PowerFx.Connectors.DatasetMetadata; // Rename

namespace CdpSampleWebApi.Controllers
{
    // For Power Apps, routes must be hosted directly at /, and not have any additional prefix. 
    // Webapp can have the extra prefix and things. It's just that the connector needs to have extra logic in Policies.xml
    // which is a runtime logic where we can update the backend url with prefixes. 
    [ApiController]
    [Route("")]
    public class CdpController : ControllerBase
    {
        private readonly ITableProviderFactory _providerFactory;

        public CdpController(ITableProviderFactory provider)
        {
            _providerFactory = provider;
        }  

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

        [HttpGet]
        [Route("datasets/{dataset}/tables")]
        public async Task<GetTablesResponse> GetTables(string dataset)
        {           
            var provider = GetProvider();
            var result = await provider.GetTablesAsync(dataset, CancellationToken.None).ConfigureAwait(true);
            return result;
        }

        // https://msazure.visualstudio.com/OneAgile/_wiki/wikis/OneAgile.wiki/715795/Connectors-(Internal)?anchor=%26%23x25b6%3B-get-table-metadata-(schema)
        [HttpGet]
        [Route("$metadata.json/datasets/{dataset}/tables/{tableName}")]
        public async Task<GetTableResponse> GetTableInfo(
            string dataset,
            string tableName,
            [FromQuery(Name = "api-version")] string apiVersion,
            [FromQuery(Name = "extractSensitivityLabel")] bool extractSensitivityLabel = false,
            [FromQuery(Name = "purviewAccountName")] string purviewAccountName = null)
        {
            var provider = GetProvider();
            var metadataSetting = new TableMetadataSetting(extractSensitivityLabel, purviewAccountName);
            RecordType record = await provider.GetTableAsync(dataset, tableName, CancellationToken.None).ConfigureAwait(true);

            GetTableResponse resp = record.ToTableResponse(tableName, metadataSetting);

            return resp;
        }

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

        private ITableProvider GetProvider()
        {
            var req = this.Request;

            if (req.Headers.TryGetValue("authorization", out var values))
            {
                var authToken = values.First();

                // Token is a base64 property bag
                var json = Convert.FromBase64String(authToken);
                var settings = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

                ITableProvider provider = this._providerFactory.Get(settings);
                return provider;
            }

            if (_providerFactory is TrivialTableProviderFactory)
            {
                ITableProvider provider = _providerFactory.Get(null);
                return provider;
            }

            throw new InvalidOperationException($"No auth");
        }
    }
}
