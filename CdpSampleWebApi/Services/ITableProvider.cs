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
        /// <summary>
        /// Returns the list of datasets (logical name, display name).
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>An array of dataset items.</returns>
        public Task<DatasetResponse.Item[]> GetDatasetsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns the list of tables for a given dataset.
        /// </summary>
        /// <param name="dataset">The dataset name.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The tables response.</returns>
        public Task<GetTablesResponse> GetTablesAsync(string dataset, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns the record type for a given table.
        /// </summary>
        /// <param name="dataset">The dataset name.</param>
        /// <param name="tableName">The table name.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The record type for the table.</returns>
        public Task<RecordType> GetTableAsync(string dataset, string tableName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns the table value for a given table.
        /// </summary>
        /// <param name="dataset">The dataset name.</param>
        /// <param name="tableName">The table name.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The table value.</returns>
        public Task<TableValue> GetTableValueAsync(string dataset, string tableName, CancellationToken cancellationToken = default);
    }
}
