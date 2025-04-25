// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace CdpValidator
{
#pragma warning disable SA1300 // Element should begin with upper-case letter

    /// <summary>
    /// Describe a CDP connection that we want to inspect. 
    /// This could be to either a localhost endpoint; or to a live CDP endpoint.
    /// </summary>
    public class CdpInternalConnection
    {
        public string endpoint { get; set; }

        public string environmentId { get; set; }

        public string connectionId { get; set; }

        // Eg: (connectionId will get replaced)
        // "/apim/sharepointonline/{connectionId}",
        public string urlprefix { get; set; }

        public string dataset { get; set; }

        public string tablename { get; set; }

        // Better way to do auth?
        public string jwtFile { get; set; }
    }
}
