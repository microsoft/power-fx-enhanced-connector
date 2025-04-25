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
    /// Exception thrown when on CDP failures. 
    /// This implies a bug in the CDP connector (giving bad data).
    /// $$$ Add these to PowerFxConnectorException? 
    /// </summary>
    public class CdpException : Exception
    {
        public HttpRequestMessage Request { get; }

        public HttpResponseMessage Response { get; }

        public ErrorResponse ErrorResponse { get;  }

        public CdpException(string message, HttpRequestMessage req, HttpResponseMessage response, ErrorResponse errorResponse)
            : base(message)
        {
            this.Request = req;
            this.Response = response;
            this.ErrorResponse = errorResponse;
        }
    }
}
