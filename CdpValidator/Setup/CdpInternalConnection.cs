// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace CdpValidator
{
#pragma warning disable SA1300 // Element should begin with upper-case letter

    /// <summary>
    /// Describes a CDP connection to inspect, which can be either a localhost or a live CDP endpoint.
    /// </summary>
    public class CdpInternalConnection
    {
        /// <summary>
        /// Gets or sets the endpoint URL for the CDP connection.
        /// </summary>
        public string endpoint { get; set; }

        /// <summary>
        /// Gets or sets the environment ID for the CDP connection.
        /// </summary>
        public string environmentId { get; set; }

        /// <summary>
        /// Gets or sets the connection ID for the CDP connection.
        /// </summary>
        public string connectionId { get; set; }

        /// <summary>
        /// Gets or sets the URL prefix for the connection. The {connectionId} token will be replaced.
        /// Example: "/apim/sharepointonline/{connectionId}"
        /// </summary>
        public string urlprefix { get; set; }

        /// <summary>
        /// Gets or sets the dataset name for the connection.
        /// </summary>
        public string dataset { get; set; }

        /// <summary>
        /// Gets or sets the table name for the connection.
        /// </summary>
        public string tablename { get; set; }

        /// <summary>
        /// Gets or sets the path to the JWT file used for authentication.
        /// </summary>
        public string jwtFile { get; set; }
    }
}
