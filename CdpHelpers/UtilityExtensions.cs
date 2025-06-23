// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using CdpHelpers.Protocol;
using Microsoft.PowerFx.Core.Functions.Delegation;
using Microsoft.PowerFx.Types;

namespace Microsoft.PowerFx.Connectors
{
    // extension methods for CapabilitiesPoco
    internal static class UtilityExtensions
    {
        // C:\dev\power-fx\src\libraries\Microsoft.PowerFx.Connectors\Tabular\Capabilities\ServiceCapabilities.cs
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

        public static IEnumerable<string> ToStr(this IEnumerable<DelegationOperator> ops)
        {
            if (ops == null)
            {
                return null;
            }

            return ops.Select(op => op.ToString().ToLowerInvariant());
        }

        // Convert from Dict<string,object> --> Dict<string,string>
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

        internal static CDPSensitivityLabelInfoCopy GetMockSensitivityLabelInfo()
        {
            var result = new CDPSensitivityLabelInfoCopy
            {
                SensitivityLabelId = Guid.NewGuid().ToString(),
                Name = "Microsoft All Employees",
                DisplayName = "Confidential \\ Microsoft Extended",
                Tooltip = null,
                Priority = 5,
                Color = "#FF8C00",
                IsEncrypted = false,
                IsEnabled = false,
                IsParent = false,
                ParentSensitivityLabelId = Guid.NewGuid().ToString(),
            };

            return result;
        }
    }
}
