// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System.Collections.Generic;
using Microsoft.PowerFx.Types;

namespace CdpValidator
{
    /// <summary>
    /// Provides utility extension methods for working with OptionSetValueType.
    /// </summary>
    internal static class Utility
    {
        /// <summary>
        /// Gets the display names for all logical names in the given OptionSetValueType.
        /// </summary>
        /// <param name="opt">The OptionSetValueType instance.</param>
        /// <returns>An enumerable of display names.</returns>
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
