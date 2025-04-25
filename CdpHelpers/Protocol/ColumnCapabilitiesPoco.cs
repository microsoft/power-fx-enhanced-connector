// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Linq;
using Microsoft.PowerFx.Core.Functions.Delegation;

#pragma warning disable SA1300, IDE1006 // Elements should begin with an upper case

namespace Microsoft.PowerFx.Connectors
{
    public class ColumnCapabilitiesPoco
    {
        // DelegationOperator
        public string[] filterFunctions { get; set; }

        // Strong-typing
        public IEnumerable<DelegationOperator> filterFunctionOps() =>
            UtilityExtensions.ToOp(filterFunctions);

        public static ColumnCapabilitiesPoco New(IEnumerable<DelegationOperator> ops)
        {
            return new ColumnCapabilitiesPoco
            {
                filterFunctions = ops.ToStr().ToArray()
            };
        }
    }
}
