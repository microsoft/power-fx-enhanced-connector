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
        public override bool IsDelegable => true;

        private static readonly IReadOnlyList<DelegationOperator> _commonSupportedDelegation = new List<DelegationOperator> { DelegationOperator.Eq, DelegationOperator.Ne, DelegationOperator.Lt, DelegationOperator.Le, DelegationOperator.Gt, DelegationOperator.Ge, DelegationOperator.And, DelegationOperator.Or, DelegationOperator.Top };

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
#pragma warning disable CS0618 // Type or member is obsolete
            CountCapabilities = new MockCountCapabilities();
#pragma warning restore CS0618 // Type or member is obsolete
        }

        public override ColumnCapabilitiesDefinition GetColumnCapability(string fieldName)
        {
            return new ColumnCapabilitiesDefinition()
            {
                FilterFunctions = _commonSupportedDelegation
            };
        }

        [Obsolete]
        private class MockCountCapabilities
            : CountCapabilities
        {
            public MockCountCapabilities()
            {
            }

            public override bool IsCountableAfterFilter()
            {
                return true;
            }

            public override bool IsCountableTable()
            {
                return true;
            }
        }
    }
}
