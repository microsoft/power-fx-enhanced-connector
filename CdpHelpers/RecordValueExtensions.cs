// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System.Collections.Generic;
using Microsoft.PowerFx.Types;

#pragma warning disable SA1300, IDE1006 // Elements should begin with an upper case

namespace Microsoft.PowerFx.Connectors
{
    /// <summary>
    /// Extension methods for <see cref="RecordValue"/> to support OData value extraction.
    /// </summary>
    public static class RecordValueExtensions
    {
        /// <summary>
        /// Extracts a dictionary of OData-compatible primitive values from a <see cref="RecordValue"/>.
        /// Only string, number, and decimal fields are included. Relationships and unsupported types are skipped.
        /// </summary>
        /// <param name="value">The <see cref="RecordValue"/> to extract values from.</param>
        /// <returns>A dictionary mapping field names to their primitive values.</returns>
        public static Dictionary<string, object> GetODataValues(this RecordValue value)
        {
            var v1 = new Dictionary<string, object>();

            foreach (var fieldValue in value.Fields)
            {
                string fieldName = fieldValue.Name;

                if (fieldValue.IsExpandEntity)
                {
                    // $$$ Relationships are not supported.
                    continue;
                }

                // $$$ !?!?! Why is this throwing...
                if (fieldValue.Value.Type == FormulaType.String ||
                    fieldValue.Value.Type == FormulaType.Number ||
                    fieldValue.Value.Type == FormulaType.Decimal)
                {
                    // ok
                }
                else
                {
                    continue; // $$$ Marshalling
                }

                FormulaValue x = fieldValue.Value;

                if (x.TryGetPrimitiveValue(out var obj))
                {
                    if (obj != null)
                    {
                        v1[fieldName] = obj;
                    }
                }
            }

            return v1;
        }
    }
}
