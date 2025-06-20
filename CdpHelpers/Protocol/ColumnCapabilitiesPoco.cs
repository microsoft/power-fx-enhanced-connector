// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Linq;
using Microsoft.PowerFx.Core.Functions.Delegation;

#pragma warning disable SA1300, IDE1006 // Elements should begin with an upper case

namespace Microsoft.PowerFx.Connectors
{
    /// <summary>
    /// Describes column-level capabilities for OData and Power Fx connectors, such as supported filter functions.
    /// </summary>
    public class ColumnCapabilitiesPoco
    {
        /// <summary>
        /// Gets or sets the supported filter functions for the column (e.g., "eq", "and", "or").
        /// </summary>
        public string[] filterFunctions { get; set; }

        /// <summary>
        /// Gets the supported filter functions as <see cref="DelegationOperator"/> values.
        /// </summary>
        /// <returns>An enumerable of <see cref="DelegationOperator"/> values.</returns>
        public IEnumerable<DelegationOperator> filterFunctionOps() =>
            UtilityExtensions.ToOp(filterFunctions);

        /// <summary>
        /// Creates a new <see cref="ColumnCapabilitiesPoco"/> from a list of <see cref="DelegationOperator"/> values.
        /// </summary>
        /// <param name="ops">The supported delegation operators.</param>
        /// <returns>A new <see cref="ColumnCapabilitiesPoco"/> instance.</returns>
        public static ColumnCapabilitiesPoco New(IEnumerable<DelegationOperator> ops)
        {
            return new ColumnCapabilitiesPoco
            {
                filterFunctions = ops.ToStr().ToArray()
            };
        }
    }
}
