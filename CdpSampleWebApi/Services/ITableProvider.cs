// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using Microsoft.PowerFx.Connectors;
using Microsoft.PowerFx.Types;

namespace CdpSampleWebApi
{
    // Helper to get a provider for a given auth token. 
    public interface ITableProviderFactory
    {
        ITableProvider Get(IReadOnlyDictionary<string, string> settings);
    }

    /// <summary>
    /// Datasource Provider. Host implements this to provide the RecordTypes for a specific source.
    /// </summary>
    public interface ITableProvider
    {
        // Return list of datasets (logical name, display name)
        public Task<DatasetResponse.Item[]> GetDatasetsAsync(CancellationToken cancellationToken = default);

        // Provider list of the tables.
        public Task<GetTables> GetTablesAsync(string dataset, CancellationToken cancellationToken = default);

        public Task<RecordType> GetTableAsync(string dataset, string tableName, CancellationToken cancellationToken = default);

        public Task<TableValue> GetTableValueAsync(string dataset, string tableName, CancellationToken cancellationToken = default);
    }
}
