// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System.Collections.Generic;

#pragma warning disable SA1300, IDE1006 // Elements should begin with an upper case
#pragma warning disable CA1002 // Do not expose generic lists

namespace Microsoft.PowerFx.Connectors
{
    /// <summary>
    /// Represents the response for a GetItems operation, containing a list of item values.
    /// </summary>
    public class GetItemsResponse
    {
        /// <summary>
        /// Gets or sets the list of item values.
        /// Each value is a dictionary mapping property names to property values (number, string, or date).
        /// </summary>
        public List<Dictionary<string, object>> value { get; set; }
    }
}
