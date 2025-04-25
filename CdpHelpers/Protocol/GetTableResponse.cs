// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System.Text.Json.Serialization;

#pragma warning disable SA1300, IDE1006 // Elements should begin with an upper case

namespace Microsoft.PowerFx.Connectors
{
    // $$$ These don't exist in core, but should. 
    public class GetTableResponse
    {
        public string name { get; set; }

        [JsonPropertyName("x-ms-permission")]
        public string permissions { get; set; } // read-write

        // $$$ Fill in rest of stuff here...
        [JsonPropertyName("x-ms-capabilities")]
        public CapabilitiesPoco capabilities { get; set; }

        public TableSchemaPoco schema { get; set; }
    }
}