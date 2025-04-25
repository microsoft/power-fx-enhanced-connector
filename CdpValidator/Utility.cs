// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System.Collections.Generic;
using Microsoft.PowerFx.Types;

namespace CdpValidator
{
    internal static class Utility
    {
        public static IEnumerable<string> DisplayNames(this OptionSetValueType opt)
        {
            foreach (var logical in opt.LogicalNames)
            {
                opt.TryGetValue(logical, out var osValue);
                string display = osValue.DisplayName;
                yield return display;
            }
        }
    }
}
