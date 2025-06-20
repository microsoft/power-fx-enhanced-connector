// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Text.Json.Serialization;

#pragma warning disable SA1300, IDE1006 // Elements should begin with an upper case
#pragma warning disable CA1708 // Identifiers should differ by more than case
#pragma warning disable CA1034 // Do not nest type Items

namespace Microsoft.PowerFx.Connectors
{
    /// <summary>
    /// Describes the schema of a table, including its type and column definitions. Included in <see cref="GetTableResponse"/>.
    /// </summary>
    public class TableSchemaPoco
    {
        /// <summary>
        /// Gets or sets the type of the schema (default is "array").
        /// </summary>
        public string type { get; set; } = "array";

        /// <summary>
        /// Gets or sets the items definition, which describes the columns of the table.
        /// </summary>
        public Items items { get; set; }

        /// <summary>
        /// Describes the items (columns) in the table schema.
        /// </summary>
        public class Items
        {
            /// <summary>
            /// Gets or sets the type of the items (default is "object").
            /// </summary>
            public string type { get; set; } = "object";

            /// <summary>
            /// Gets or sets the dictionary of column properties, keyed by column name.
            /// </summary>
            public Dictionary<string, ColumnInfo> properties { get; set; }
        }

        /// <summary>
        /// Describes a column in the table schema, including title, description, type, sort, and capabilities.
        /// </summary>
        public class ColumnInfo
        {
            /// <summary>
            /// Gets or sets the title of the column.
            /// </summary>
            public string title { get; set; }

            /// <summary>
            /// Gets or sets the description of the column.
            /// </summary>
            public string description { get; set; }

            /// <summary>
            /// Gets or sets the type of the column (e.g., "integer", "string").
            /// </summary>
            public string type { get; set; } // "integer", "string"

            /// <summary>
            /// Gets or sets the sort capabilities for the column (e.g., "asc,desc").
            /// </summary>
            [JsonPropertyName("x-ms-sort")]
            public string sort { get; set; }

            /// <summary>
            /// Gets or sets the filter capabilities for the column.
            /// </summary>
            [JsonPropertyName("x-ms-capabilities")]
            public ColumnCapabilitiesPoco capabilities { get; set; }
        }
    }
}
