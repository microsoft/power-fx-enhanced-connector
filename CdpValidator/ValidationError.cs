// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.Diagnostics;

namespace CdpValidator
{
    /// <summary>
    /// Represents an error or warning produced by the validation tool.
    /// </summary>
    [DebuggerDisplay("{Message}")]
    public class ValidationError
    {        
        /// <summary>
        /// Gets or sets the most relevant URI from the connector.
        /// </summary>
        public string Uri { get; set; }

        /// <summary>
        /// Gets or sets a short description of the error or warning (e.g., "Name is blank").
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets a longer description of the error or warning.
        /// </summary>
        public string Details { get; set; }

        /// <summary>
        /// Gets or sets the exception associated with the error, if any.
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// Gets or sets the validation category.
        /// </summary>
        public ValidationCategory Category { get; set; }
    }

    /// <summary>
    /// Specifies the category of a validation result.
    /// </summary>
    public enum ValidationCategory
    {
        /// <summary>
        /// Indicates an error.
        /// </summary>
        Error,

        /// <summary>
        /// Indicates a warning.
        /// </summary>
        Warning
    }
}