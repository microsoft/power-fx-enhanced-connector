// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Text.Json.Serialization;
using CdpHelpers.Protocol;

#pragma warning disable SA1300, IDE1006 // Elements should begin with an upper case
#pragma warning disable CA1708 // Identifiers should differ by more than case
#pragma warning disable CA1034 // Do not nest type Items

namespace Microsoft.PowerFx.Connectors
{
    /// <summary>
    /// Included in <see cref="GetTableResponse"/>.
    /// </summary>
    public class TableSchemaPoco
    {
        public string type { get; set; } = "array";

        public Items items { get; set; }

        public class Items
        {
            public string type { get; set; } = "object";

            public Dictionary<string, ColumnInfo> properties { get; set; }
        }

        public class ColumnInfo
        {
            public string title { get; set; }

            public string description { get; set; }

            public string type { get; set; } // $$$ "integer", "string",

            // What sorting capabilities?
            // "asc,desc"
            [JsonPropertyName("x-ms-sort")]
            public string sort { get; set; }

            // What filter capabilities?
            [JsonPropertyName("x-ms-capabilities")]
            public ColumnCapabilitiesPoco capabilities { get; set; }

            [JsonPropertyName("x-ms-content-sensitivityLabelInfo")]
            public IEnumerable<CDPSensitivityLabelInfoCopy> sensitivityLabels { get; set; }
        }
    }
}
