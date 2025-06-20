// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.PowerFx.Core.Functions.Delegation;

namespace Microsoft.PowerFx.Connectors
{
    /// <summary>
    /// Extension methods for CapabilitiesPoco and related types.
    /// </summary>
    internal static class UtilityExtensions
    {
        /// <summary>
        /// Converts a list of filter function names to a list of <see cref="DelegationOperator"/> values.
        /// </summary>
        /// <param name="filterFunctionList">The list of filter function names.</param>
        /// <returns>A list of <see cref="DelegationOperator"/> values, or null if input is null.</returns>
        public static IEnumerable<DelegationOperator> ToOp(IEnumerable<string> filterFunctionList)
        {
            if (filterFunctionList == null)
            {
                return null;
            }

            List<DelegationOperator> list = new List<DelegationOperator>();

            foreach (string str in filterFunctionList)
            {
                if (Enum.TryParse(str, true, out DelegationOperator op))
                {
                    list.Add(op);
                }
            }

            return list;
        }

        /// <summary>
        /// Converts a list of <see cref="DelegationOperator"/> values to their lowercase string representations.
        /// </summary>
        /// <param name="ops">The list of operators.</param>
        /// <returns>A list of lowercase string representations, or null if input is null.</returns>
        public static IEnumerable<string> ToStr(this IEnumerable<DelegationOperator> ops)
        {
            if (ops == null)
            {
                return null;
            }

            return ops.Select(op => op.ToString().ToLowerInvariant());
        }

        /// <summary>
        /// Converts a dictionary of string/object pairs to a dictionary of string/string pairs.
        /// </summary>
        /// <param name="dict">The input dictionary.</param>
        /// <returns>A dictionary with string keys and string values.</returns>
        internal static IDictionary<string, string> ToStrDict(this IReadOnlyDictionary<string, object> dict)
        {
            IDictionary<string, string> options = new Dictionary<string, string>();
            foreach (var kv in dict)
            {
                var val = kv.Value;
                if (val != null)
                {
                    string str;
                    if (val is IEnumerable<string> list)
                    {
                        str = string.Join(",", list);
                    }
                    else
                    {
                        str = kv.Value.ToString();
                    }

                    options[kv.Key] = str;
                }
            }

            return options;
        }
    }
}
