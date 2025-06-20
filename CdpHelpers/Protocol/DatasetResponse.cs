// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System.Text.Json.Serialization;

#pragma warning disable SA1300, IDE1006 // Elements should begin with an upper case
#pragma warning disable CA1034 // Do not nest type Item

namespace Microsoft.PowerFx.Connectors
{        
    /// <summary>
    /// Represents a response containing a list of datasets for OData and Power Fx connectors.
    /// </summary>
    public class DatasetResponse
    {
        /// <summary>
        /// Gets or sets the array of dataset items.
        /// </summary>
        public Item[] value { get; set; }

        /// <summary>
        /// Represents a dataset item with name and display name properties.
        /// </summary>
        public class Item
        {
            /// <summary>
            /// Gets or sets the logical name of the dataset.
            /// </summary>
            [JsonPropertyName("Name")]
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the display name of the dataset.
            /// </summary>
            [JsonPropertyName("DisplayName")]
            public string DisplayName { get; set; }
        }
    }
}
