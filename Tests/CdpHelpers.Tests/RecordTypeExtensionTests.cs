// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System.Collections.Generic;
using Microsoft.PowerFx.Connectors;
using Microsoft.PowerFx.Types;

namespace CdpHelpers.Tests
{
    public class RecordTypeExtensionTests
    {
        [Fact]
        public void Test1()
        {
            var odataParameters = new Dictionary<string, string>
            {
                { "$filter", "score gt 50" },
                { "$top", "2" },
                { "$select", "name" }
            };

            RecordType recordType = RecordType.Empty()
                .Add("score", FormulaType.Number)
                .Add("name", FormulaType.String);

            var delegation = recordType.Convert(odataParameters);

            Assert.Equal(delegation.Top, 2);

            string select = string.Join(",", delegation.GetColumns());
            Assert.Equal("name", select);

            string filter = delegation.GetOdataFilter();
            Assert.Equal("score gt 50", filter);
        }
    }
}
