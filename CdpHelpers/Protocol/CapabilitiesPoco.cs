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
    /// Included in <see cref="GetTableResponse"/>.
    /// $$$ See ServiceCapabilities in Fx.
    /// </summary>
    public class CapabilitiesPoco
    {
        public Sort sortRestrictions { get; set; }

        public Filter filterRestrictions { get; set; }

        public bool isOnlyServerPagable { get; set; }

        // "top", "skiptoken"
        public string[] serverPagingOptions { get; set; }

        // and,or,eq, etc...

        // DelegationOperator
        public string[] filterFunctionSupport { get; set; }

        public CapabilitiesPoco SetOps(IEnumerable<DelegationOperator> ops)
        {
            this.filterFunctionSupport = ops.ToStr().ToArray();
            return this;
        }

        public IEnumerable<DelegationOperator> filterFunctionSupportOps() => UtilityExtensions.ToOp(filterFunctionSupport);

        // 3
        public int odataVersion { get; set; }

        public class Filter
        {
            public bool filterable { get; set; }

            public string[] nonFilterableProperties { get; set; }
        }

        public class Sort
        {
            public bool sortable { get; set; }

            public string[] unsortableProperties { get; set; }
        }
    }
}
