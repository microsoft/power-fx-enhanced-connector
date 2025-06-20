// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

#pragma warning disable CS0618 // Type or member is obsolete

using System.Collections.Generic;
using Microsoft.PowerFx.Dataverse;
using Microsoft.PowerFx.Types;

namespace CdpValidator
{
    /// <summary>
    /// Delegate for validating two objects for equality or custom logic.
    /// </summary>
    internal delegate bool ValidateObject(object x, object y);

    /// <summary>
    /// Represents delegation parameters for validation, allowing custom filters and column selection.
    /// </summary>
    internal class ValidationDelegationParameters : DataverseDelegationParameters
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationDelegationParameters"/> class.
        /// </summary>
        /// <param name="expectedReturnType">The expected return type for the delegation.</param>
        public ValidationDelegationParameters(FormulaType expectedReturnType)
            : base(expectedReturnType)
        {
        }

        /// <summary>
        /// Gets the delegation parameter features. Only Top is supported.
        /// </summary>
        public override DelegationParameterFeatures Features => DelegationParameterFeatures.Top;

        /// <summary>
        /// Allows injection of any filter. This filter will later be URI-encoded.
        /// </summary>
        public string RawFilter { get; init; }

        /// <summary>
        /// Gets or sets the delegate used to validate objects.
        /// </summary>
        public ValidateObject Validate;

        /// <summary>
        /// Gets the OData filter string for the query.
        /// </summary>
        /// <returns>The OData filter string.</returns>
        public override string GetOdataFilter()
        {
            return !string.IsNullOrEmpty(RawFilter) ? RawFilter : base.GetOdataFilter();
        }

        /// <summary>
        /// Gets or sets the columns to select in the query.
        /// </summary>
        internal IReadOnlyCollection<string> Columns { get; init; }

        /// <summary>
        /// Gets the columns to select in the query.
        /// </summary>
        /// <returns>The collection of column names.</returns>
        public override IReadOnlyCollection<string> GetColumns()
        {
            return Columns;
        }
    }
}

#pragma warning restore CS0618 // Type or member is obsolete
