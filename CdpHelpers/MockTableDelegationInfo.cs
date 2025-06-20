// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.PowerFx.Core.Entities;
using Microsoft.PowerFx.Core.Functions.Delegation;
using Microsoft.PowerFx.Types;

namespace CdpHelpers
{
    /// <summary>
    /// MockTableDelegationInfo is a mock implementation of TableDelegationInfo. This should Ideally come from <see cref="RecordType.TryGetCapabilities(out TableDelegationInfo)"/> if you have custom record type implementation.
    /// </summary>
    internal class MockTableDelegationInfo : TableDelegationInfo
    {
        /// <summary>
        /// Gets a value indicating whether the table is delegable.
        /// </summary>
        public override bool IsDelegable => true;

        /// <summary>
        /// Common set of supported delegation operators for this mock implementation.
        /// </summary>
        private static readonly IReadOnlyList<DelegationOperator> _commonSupportedDelegation = new List<DelegationOperator> { DelegationOperator.Eq, DelegationOperator.Ne, DelegationOperator.Lt, DelegationOperator.Le, DelegationOperator.Gt, DelegationOperator.Ge, DelegationOperator.And, DelegationOperator.Or, DelegationOperator.Top };

        /// <summary>
        /// Initializes a new instance of the <see cref="MockTableDelegationInfo"/> class.
        /// </summary>
        public MockTableDelegationInfo()
        {
            FilterSupportedFunctions = _commonSupportedDelegation;
            FilterRestriction = new FilterRestrictions
            {
                NonFilterableProperties = new List<string> { },
            };
            SortRestriction = new SortRestrictions
            {
                UnsortableProperties = new List<string> { }
            };
        }

        /// <summary>
        /// Gets the column capability definition for a given field name.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <returns>A <see cref="ColumnCapabilitiesDefinition"/> with supported filter functions.</returns>
        public override ColumnCapabilitiesDefinition GetColumnCapability(string fieldName)
        {
            return new ColumnCapabilitiesDefinition()
            {
                FilterFunctions = _commonSupportedDelegation
            };
        }
    }
}
