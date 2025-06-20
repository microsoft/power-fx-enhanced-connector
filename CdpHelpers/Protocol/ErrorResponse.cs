// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.PowerFx.Connectors
{
    /// <summary>
    /// Represents a standard error response payload for connectors.
    /// </summary>
    /// <remarks>
    /// Example: { "statusCode": 404, "message": "Resource not found" }
    /// </remarks>
    public class ErrorResponse
    {
#pragma warning disable SA1300 // Element should begin with upper-case letter
        /// <summary>
        /// Gets or sets the HTTP status code for the error.
        /// </summary>
        public int statusCode { get; set; }

        /// <summary>
        /// Gets or sets a descriptive message of the error.
        /// </summary>
        public string message { get; set; }
#pragma warning restore SA1300 // Element should begin with upper-case letter
    }
}
