// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.PowerFx.Connectors;

namespace Microsoft.PowerFx.Connectors
{
    /// <summary>
    /// Exception thrown on CDP failures, indicating a bug in the CDP connector (bad data).
    /// </summary>
    public class CdpException : Exception
    {
        /// <summary>
        /// Gets the HTTP request message that caused the exception.
        /// </summary>
        public HttpRequestMessage Request { get; }

        /// <summary>
        /// Gets the HTTP response message associated with the exception.
        /// </summary>
        public HttpResponseMessage Response { get; }

        /// <summary>
        /// Gets the error response payload returned by the connector.
        /// </summary>
        public ErrorResponse ErrorResponse { get;  }

        /// <summary>
        /// Initializes a new instance of the <see cref="CdpException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="req">The HTTP request message.</param>
        /// <param name="response">The HTTP response message.</param>
        /// <param name="errorResponse">The error response payload.</param>
        public CdpException(string message, HttpRequestMessage req, HttpResponseMessage response, ErrorResponse errorResponse)
            : base(message)
        {
            this.Request = req;
            this.Response = response;
            this.ErrorResponse = errorResponse;
        }
    }
}
