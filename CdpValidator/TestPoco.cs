// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System.Collections.ObjectModel;

#pragma warning disable CA1034 // Nested types should not be visible

namespace CdpValidator
{
    // Describe Validation test schema
    public class TestPoco
    {
        public string Connection { get; set; }

        public GetItemsTests GetItemsTests { get; set; }
    }

    public class GetItemsTests
    {
        // Common properties for each test here
        public string Dataset { get; set; }

        public string TableName { get; set; }

        public Collection<TestCase> Tests { get; set; }

        public class TestCase
        {
            public string Filter { get; set; }

            public int? Top { get; set; }

            // Orderby
            // Select
        }
    }
}
