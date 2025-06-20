// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System.Globalization;
using Microsoft.AspNetCore.Mvc;

#pragma warning disable SA1300, IDE1006 // Elements should begin with an upper case

namespace CdpSampleWebApi.Services
{
    /// <summary>
    /// Represents OData parameters in the query string for model binding in ASP.NET.
    /// Collects parameters from the URI but does not parse them.
    /// </summary>
    public class ODataQueryModel
    {
        /// <summary>
        /// Gets or sets the $select OData parameter.
        /// </summary>
        [FromQuery(Name = "$select")]
        public string Select { get; set; }

        /// <summary>
        /// Gets or sets the $filter OData parameter.
        /// </summary>
        [FromQuery(Name = "$filter")]
        public string Filter { get; set; }

        /// <summary>
        /// Gets or sets the $top OData parameter.
        /// </summary>
        [FromQuery(Name = "$top")]
        public int top { get; set; }

        /// <summary>
        /// Gets or sets the $orderby OData parameter.
        /// </summary>
        [FromQuery(Name = "$orderby")]
        public string orderby { get; set; }

        /// <summary>
        /// Converts the OData query model to a dictionary of string key-value pairs.
        /// </summary>
        /// <returns>A dictionary containing the OData parameters as key-value pairs.</returns>
        public IDictionary<string, string> ToStrDict()
        {
            var d = new Dictionary<string, string>();
            Add(d, "$select", this.Select);
            Add(d, "$filter", this.Filter);
            Add(d, "$orderby", this.orderby);
            if (top > 0)
            {
                Add(d, "$top", this.top.ToString(CultureInfo.InvariantCulture));
            }

            return d;
        }

        /// <summary>
        /// Adds a key-value pair to the dictionary if the value is not null.
        /// </summary>
        /// <param name="d">The dictionary to add to.</param>
        /// <param name="key">The key to add.</param>
        /// <param name="value">The value to add.</param>
        private void Add(Dictionary<string, string> d, string key, string value)
        {
            if (value != null)
            {
                d[key] = value;
            }
        }
    }
}
