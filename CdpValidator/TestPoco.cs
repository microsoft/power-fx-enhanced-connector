// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System.Collections.ObjectModel;

#pragma warning disable CA1034 // Nested types should not be visible

namespace CdpValidator
{
    /// <summary>
    /// Describes the validation test schema for a connection and its item tests.
    /// </summary>
    public class TestPoco
    {
        /// <summary>
        /// Gets or sets the connection string or identifier.
        /// </summary>
        public string Connection { get; set; }

        /// <summary>
        /// Gets or sets the tests for getting items.
        /// </summary>
        public GetItemsTests GetItemsTests { get; set; }
    }

    /// <summary>
    /// Represents a set of tests for retrieving items from a dataset and table.
    /// </summary>
    public class GetItemsTests
    {
        /// <summary>
        /// Gets or sets the dataset name for the test.
        /// </summary>
        public string Dataset { get; set; }

        /// <summary>
        /// Gets or sets the table name for the test.
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// Gets or sets the collection of test cases.
        /// </summary>
        public Collection<TestCase> Tests { get; set; }

        /// <summary>
        /// Represents a single test case for item retrieval.
        /// </summary>
        public class TestCase
        {
            /// <summary>
            /// Gets or sets the filter expression for the test case.
            /// </summary>
            public string Filter { get; set; }

            /// <summary>
            /// Gets or sets the maximum number of items to retrieve.
            /// </summary>
            public int? Top { get; set; }

            // Orderby
            // Select
        }
    }
}
