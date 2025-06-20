// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Text.Json.Serialization;

#pragma warning disable SA1300, IDE1006 // Elements should begin with an upper case
#pragma warning disable CA1002 // Do not expose generic lists

namespace Microsoft.PowerFx.Connectors
{
    /// <summary>
    /// Represents the response for a GetTables operation, containing a list of raw table information.
    /// </summary>
    public class GetTablesResponse
    {
        /// <summary>
        /// Gets or sets the list of raw table information.
        /// </summary>
        [JsonPropertyName("value")]
        public List<RawTablePoco> Value { get; set; }
    }

    /// <summary>
    /// Represents raw table information, including logical and display names.
    /// </summary>
    public class RawTablePoco
    {
        /// <summary>
        /// Gets or sets the logical name of the table.
        /// </summary>
        [JsonPropertyName("Name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the display name of the table.
        /// </summary>
        [JsonPropertyName("DisplayName")]
        public string DisplayName { get; set; }

        /// <summary>
        /// Returns a string representation of the table, including display and logical names.
        /// </summary>
        /// <returns>A string in the format "DisplayName: Name".</returns>
        public override string ToString() => $"{DisplayName}: {Name}";
    }
}
