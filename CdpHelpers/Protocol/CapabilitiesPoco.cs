// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Linq;
using Microsoft.PowerFx.Core.Functions.Delegation;

#pragma warning disable SA1300, IDE1006 // Elements should begin with an upper case
#pragma warning disable CA1034 // Do not nest type Filter, Sort

namespace Microsoft.PowerFx.Connectors
{
    /// <summary>
    /// Describes table capabilities for OData and Power Fx connectors, including filtering, sorting, and server paging options.
    /// Included in <see cref="GetTableResponse"/>.
    /// </summary>
    public class CapabilitiesPoco
    {
        /// <summary>
        /// Gets or sets the sort restrictions for the table.
        /// </summary>
        public Sort sortRestrictions { get; set; }

        /// <summary>
        /// Gets or sets the filter restrictions for the table.
        /// </summary>
        public Filter filterRestrictions { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the table is only server pagable.
        /// </summary>
        public bool isOnlyServerPagable { get; set; }

        /// <summary>
        /// Gets or sets the server paging options (e.g., "top", "skiptoken").
        /// </summary>
        public string[] serverPagingOptions { get; set; }

        /// <summary>
        /// Gets or sets the supported filter functions (e.g., "eq", "and", "or").
        /// </summary>
        public string[] filterFunctionSupport { get; set; }

        /// <summary>
        /// Sets the supported filter functions from a list of <see cref="DelegationOperator"/> values.
        /// </summary>
        /// <param name="ops">The supported delegation operators.</param>
        /// <returns>The current <see cref="CapabilitiesPoco"/> instance.</returns>
        public CapabilitiesPoco SetOps(IEnumerable<DelegationOperator> ops)
        {
            this.filterFunctionSupport = ops.ToStr().ToArray();
            return this;
        }

        /// <summary>
        /// Gets the supported filter functions as <see cref="DelegationOperator"/> values.
        /// </summary>
        /// <returns>An enumerable of <see cref="DelegationOperator"/> values.</returns>
        public IEnumerable<DelegationOperator> filterFunctionSupportOps() => UtilityExtensions.ToOp(filterFunctionSupport);

        /// <summary>
        /// Gets or sets the OData version (e.g., 3).
        /// </summary>
        public int odataVersion { get; set; }

        /// <summary>
        /// Describes filter restrictions for a table.
        /// </summary>
        public class Filter
        {
            /// <summary>
            /// Gets or sets a value indicating whether the table is filterable.
            /// </summary>
            public bool filterable { get; set; }

            /// <summary>
            /// Gets or sets the list of non-filterable property names.
            /// </summary>
            public string[] nonFilterableProperties { get; set; }
        }

        /// <summary>
        /// Describes sort restrictions for a table.
        /// </summary>
        public class Sort
        {
            /// <summary>
            /// Gets or sets a value indicating whether the table is sortable.
            /// </summary>
            public bool sortable { get; set; }

            /// <summary>
            /// Gets or sets the list of unsortable property names.
            /// </summary>
            public string[] unsortableProperties { get; set; }
        }
    }
}
