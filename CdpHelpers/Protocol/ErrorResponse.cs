// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.PowerFx.Connectors
{
    // Connectors should return errors in a standard payload schema.
    // { "statusCode": 404, "message": "Resource not found" }
    public class ErrorResponse
    {
#pragma warning disable SA1300 // Element should begin with upper-case letter
        /// <summary>
        /// The HttpStatusCode. 
        /// </summary>
        public int statusCode { get; set; }

        /// <summary>
        /// A descriptive message of the error.
        /// </summary>
        public string message { get; set; }
#pragma warning restore SA1300 // Element should begin with upper-case letter
    }
}
