// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CdpHelpers.Protocol
{
    // This can be removed to use the original CDPSensitivityLabelInfo class from Fx Core once nugets are updated.
    public class CDPSensitivityLabelInfoCopy
    {
        [JsonPropertyName("sensitivityLabelId")]
        public string SensitivityLabelId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("displayName")]
        public string DisplayName { get; set; }

        [JsonPropertyName("tooltip")]
        public string Tooltip { get; set; }

        [JsonPropertyName("priority")]
        public int Priority { get; set; }

        [JsonPropertyName("color")]
        public string Color { get; set; }

        // These are strings in your JSON; if you'd rather parse to bool, you can add a converter.
        [JsonPropertyName("isEncrypted")]
        public bool IsEncrypted { get; set; }

        [JsonPropertyName("isEnabled")]
        public bool IsEnabled { get; set; }

        [JsonPropertyName("isParent")]
        public bool IsParent { get; set; }

        [JsonPropertyName("parentSensitivityLabelId")]
        public string ParentSensitivityLabelId { get; set; }
    }
}
