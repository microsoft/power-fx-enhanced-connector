// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Text.Json.Serialization;

#pragma warning disable SA1300, IDE1006 // Elements should begin with an upper case
#pragma warning disable CA1002 // Do not expose generic lists

namespace Microsoft.PowerFx.Connectors
{
    // Used by ConnectorDataSource.GetTablesAsync
    public class GetTablesResponse
    {
        [JsonPropertyName("value")]
        public List<RawTablePoco> Value { get; set; }
    }

    public class RawTablePoco
    {
        // Logical Name
        [JsonPropertyName("Name")]
        public string Name { get; set; }

        [JsonPropertyName("DisplayName")]
        public string DisplayName { get; set; }

        public override string ToString() => $"{DisplayName}: {Name}";
    }
}
