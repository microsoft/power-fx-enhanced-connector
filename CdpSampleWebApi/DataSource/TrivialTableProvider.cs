// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using Microsoft.PowerFx;
using Microsoft.PowerFx.Connectors;
using Microsoft.PowerFx.Types;

namespace CdpSampleWebApi
{
    /// <summary>
    /// An in-memory table provider factory for testing, returns a <see cref="TrivialTableProvider"/>.
    /// </summary>
    public class TrivialTableProviderFactory : ITableProviderFactory
    {
        /// <summary>
        /// Gets a trivial table provider instance.
        /// </summary>
        /// <param name="settings">Optional settings for the provider (not used).</param>
        /// <returns>A <see cref="TrivialTableProvider"/> instance.</returns>
        public ITableProvider Get(IReadOnlyDictionary<string, string> settings)
        {
            return new TrivialTableProvider();
        }
    }

    /// <summary>
    /// An in-memory table provider for testing. This does not require any external service connection.
    /// </summary>
    public class TrivialTableProvider : ITableProvider
    {
        private const string _tableName = "MyTable";

        private static Dictionary<string, TableValue> _tables = new Dictionary<string, TableValue>();

        /// <summary>
        /// Initializes static members of the <see cref="TrivialTableProvider"/> class.
        /// </summary>
        static TrivialTableProvider()
        {
            TypeMarshallerCache cache = new TypeMarshallerCache();
            var result = cache.Marshal(new[] 
            {
                new 
                {
                    NumField = 10,
                    StrField = "Ten"
                },
                new 
                {
                    NumField = 20,
                    StrField = "Twenty"
                },
                new 
                {
                    NumField = 30,
                    StrField = "Thirty"
                },
                new
                {
                    NumField = 40,
                    StrField = "Forty"
                },
                new
                {
                    NumField = 50,
                    StrField = "Fifty"
                },
                new
                {
                    NumField = 60,
                    StrField = "Sixty"
                },
                new
                {
                    NumField = 70,
                    StrField = "Seventy"
                },
                new
                {
                    NumField = 80,
                    StrField = "Eighty"
                },
                new
                {
                    NumField = 90,
                    StrField = "Ninety"
                },
                new
                {
                    NumField = 100,
                    StrField = "Hundred"
                }
            });

            _tables[_tableName] = (TableValue)result;
        }

        /// <summary>
        /// Gets the available datasets.
        /// </summary>
        /// <param name="cancel">A cancellation token.</param>
        /// <returns>An array of dataset items.</returns>
        public async Task<DatasetResponse.Item[]> GetDatasetsAsync(CancellationToken cancel = default)
        {
            return new DatasetResponse.Item[]
            {
                new DatasetResponse.Item
                {
                    Name = "default",
                    DisplayName = "default"
                }
            };
        }

        /// <summary>
        /// Gets the record type for a given table.
        /// </summary>
        /// <param name="dataset">The dataset name.</param>
        /// <param name="tableName">The table name.</param>
        /// <param name="cancel">A cancellation token.</param>
        /// <returns>The record type for the table.</returns>
        public async Task<RecordType> GetTableAsync(string dataset, string tableName, CancellationToken cancel = default)
        {
            var type = _tables[tableName].Type;

            return type.ToRecord();
        }

        /// <summary>
        /// Gets the list of tables for a given dataset.
        /// </summary>
        /// <param name="dataset">The dataset name.</param>
        /// <param name="cancel">A cancellation token.</param>
        /// <returns>The tables response.</returns>
        public async Task<GetTablesResponse> GetTablesAsync(string dataset, CancellationToken cancel = default)
        {
            return new GetTablesResponse
            {
                Value = new List<RawTablePoco>
                 {
                     new RawTablePoco
                     {
                         Name = _tableName,
                         DisplayName = _tableName
                     }
                 }
            };
        }

        /// <summary>
        /// Gets the table value for a given table.
        /// </summary>
        /// <param name="dataset">The dataset name.</param>
        /// <param name="tableName">The table name.</param>
        /// <param name="cancel">A cancellation token.</param>
        /// <returns>The table value.</returns>
        public async Task<TableValue> GetTableValueAsync(string dataset, string tableName, CancellationToken cancel = default)
        {
            var value = _tables[tableName];
            return value;
        }
    }
}