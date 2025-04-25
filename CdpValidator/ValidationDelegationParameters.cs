// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System.Collections.Generic;
using Microsoft.PowerFx.Dataverse;
using Microsoft.PowerFx.Types;

namespace CdpValidator
{
#pragma warning disable CS0618 // Type or member is obsolete
    internal class ValidationDelegationParameters : DataverseDelegationParameters
#pragma warning restore CS0618 // Type or member is obsolete
    {
        public ValidationDelegationParameters(FormulaType expectedReturnType)
            : base(expectedReturnType)
        {
        }

        public override DelegationParameterFeatures Features => DelegationParameterFeatures.Top;

        // Allow us to inject any filter we want
        // Note that this filter will later be Uri-encoded
        public string RawFilter { get; init; }

        public override string GetOdataFilter()
        {
            return !string.IsNullOrEmpty(RawFilter) ? RawFilter : base.GetOdataFilter();
        }

        // $select
        internal IReadOnlyCollection<string> Columns { get; init; }

        public override IReadOnlyCollection<string> GetColumns()
        {
            return Columns;
        }
    }
}
