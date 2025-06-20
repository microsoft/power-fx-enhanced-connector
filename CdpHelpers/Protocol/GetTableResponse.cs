// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System.Text.Json.Serialization;

#pragma warning disable SA1300, IDE1006 // Elements should begin with an upper case

namespace Microsoft.PowerFx.Connectors
{
    /// <summary>
    /// Represents the response for a GetTable operation, including table name, permissions, capabilities, and schema.
    /// </summary>
    public class GetTableResponse
    {
        /// <summary>
        /// Gets or sets the name of the table.
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// Gets or sets the permissions for the table (e.g., "read-write").
        /// </summary>
        [JsonPropertyName("x-ms-permission")]
        public string permissions { get; set; } // read-write

        /// <summary>
        /// Gets or sets the table capabilities, such as filtering and sorting support.
        /// </summary>
        [JsonPropertyName("x-ms-capabilities")]
        public CapabilitiesPoco capabilities { get; set; }

        /// <summary>
        /// Gets or sets the schema of the table.
        /// </summary>
        public TableSchemaPoco schema { get; set; }
    }
}