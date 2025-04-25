// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System.Collections.Generic;

#pragma warning disable SA1300, IDE1006 // Elements should begin with an upper case
#pragma warning disable CA1002 // Do not expose generic lists

namespace Microsoft.PowerFx.Connectors
{
    // GetItems response
    public class GetItemsResponse
    {
        // List of values.
        // Each value is propertyName --> propertyValue.
        // Each property value should be number, string.
        // Date is: "2024-10-25T15:23:31Z"
        public List<Dictionary<string, object>> value { get; set; }
    }
}
