// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.Diagnostics;

namespace CdpValidator
{
    // Errors from validation tool
    [DebuggerDisplay("{Message}")]
    public class ValidationError
    {        
        /// <summary>
        /// The most relevant URI from the connector.
        /// </summary>
        public string Uri { get; set; }

        /// <summary>
        /// Short description.   Ie "Name is blank".
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Longer description. 
        /// </summary>
        public string Details { get; set; }

        /// <summary>
        /// Exception.
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// Validation category.
        /// </summary>
        public ValidationCategory Category { get; set; }
    }

    public enum ValidationCategory
    {
        Error,
        Warning
    }
}