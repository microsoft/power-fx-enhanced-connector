// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CdpHelpers.Protocol
{
    /// <summary>
    /// Settings for fetching table metadata.
    /// </summary>
    public class TableMetadataSetting
    {
        private readonly bool _extractSensitivityLabel;

        public bool ExtractSensitivityLabel => _extractSensitivityLabel;

        private readonly string _purviewAccountName;

        public string PurviewAccountName => _purviewAccountName;

        public TableMetadataSetting(bool extractSensitivityLabel = false, string purviewAccountName = null)
        {
            _extractSensitivityLabel = extractSensitivityLabel;
            _purviewAccountName = purviewAccountName;
        }
    }
}
