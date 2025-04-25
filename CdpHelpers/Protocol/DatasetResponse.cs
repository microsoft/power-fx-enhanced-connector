// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System.Text.Json.Serialization;

#pragma warning disable SA1300, IDE1006 // Elements should begin with an upper case
#pragma warning disable CA1034 // Do not nest type Item

namespace Microsoft.PowerFx.Connectors
{        
    public class DatasetResponse
    {
        public Item[] value { get; set; }

        // Capital casing here is extremely important to Power apps. 
        public class Item
        {
            [JsonPropertyName("Name")]
            public string Name { get; set; }

            [JsonPropertyName("DisplayName")]
            public string DisplayName { get; set; }
        }
    }
}
