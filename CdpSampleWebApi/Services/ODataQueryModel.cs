// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System.Globalization;
using Microsoft.AspNetCore.Mvc;

#pragma warning disable SA1300, IDE1006 // Elements should begin with an upper case

namespace CdpSampleWebApi.Services
{
    /// <summary>
    /// Represent OData parameters in the query string.
    /// This uses ASP.Net's [FromQuery] attribute for easy model binding.
    /// This just collects the parameters from the URi, but does not do any parsing.
    /// </summary>
    public class ODataQueryModel
    {
        [FromQuery(Name = "$select")]
        public string Select { get; set; }

        [FromQuery(Name = "$filter")]
        public string Filter { get; set; }

        [FromQuery(Name = "$top")]
        public int top { get; set; }

        [FromQuery(Name = "$count")]
        public bool Count { get; set; }

        // field
        // field desc
        [FromQuery(Name = "$orderby")]
        public string orderby { get; set; }

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

            Add(d, "$count", this.Count.ToString(CultureInfo.InvariantCulture));

            return d;
        }

        private void Add(Dictionary<string, string> d, string key, string value)
        {
            if (value != null)
            {
                d[key] = value;
            }
        }
    }
}
