// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using Microsoft.PowerFx;
using Microsoft.PowerFx.Connectors;
using Microsoft.PowerFx.Types;

namespace CdpSampleWebApi
{
    public class TrivialTableProviderFactory : ITableProviderFactory
    {
        public ITableProvider Get(IReadOnlyDictionary<string, string> settings)
        {
            return new TrivialTableProvider();
        }
    }

    // An in-memory table for testing. 
    // This doesn't require any external service connection. 
    public class TrivialTableProvider : ITableProvider
    {
        private const string _tableName = "MyTable";

        private static Dictionary<string, TableValue> _tables = new Dictionary<string, TableValue>();

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
            });

            _tables[_tableName] = (TableValue)result;
        }

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

        public async Task<RecordType> GetTableAsync(string dataset, string tableName, CancellationToken cancel = default)
        {
            var type = _tables[tableName].Type;

            return type.ToRecord();
        }

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

        public async Task<TableValue> GetTableValueAsync(string dataset, string tableName, CancellationToken cancel = default)
        {
            var value = _tables[tableName];
            return value;
        }
    }
}