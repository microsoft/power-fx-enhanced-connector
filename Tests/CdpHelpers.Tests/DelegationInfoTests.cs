// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using Microsoft.PowerFx.Connectors;
using Microsoft.PowerFx.Core;
using Microsoft.PowerFx.Core.Entities;
using Microsoft.PowerFx.Core.Functions.Delegation;
using Microsoft.PowerFx.Tests;
using Microsoft.PowerFx.Types;

namespace CdpHelpers.Tests
{
    /// <summary>
    /// Unit tests for table delegation info and table response serialization.
    /// </summary>
    public class DelegationInfoTests
    {
        /// <summary>
        /// Tests the ToTableResponse extension method for a non-delegated record type.
        /// </summary>
        [Fact]
        public void TestToTableResponse()
        {
            var recordType = RecordType.Empty()
                .Add("fieldStr", FormulaType.String)
                .Add("fieldNum", FormulaType.Number);

            var resp = recordType.ToTableResponse(tableName: null);
            var json = JsonNormalizer.Serialize(resp);

            Assert.Equal(
"""
{
  "schema": {
    "items": {
      "properties": {
        "fieldNum": {
          "title": "",
          "type": "integer"
        },
        "fieldStr": {
          "title": "",
          "type": "string"
        }
      },
      "type": "object"
    },
    "type": "array"
  },
  "x-ms-permission": "read-write"
}
""", json);
        }

        /// <summary>
        /// Tests the ToTableResponse extension method for a record type with delegation info.
        /// </summary>
        [Fact]
        public void TestWithDelegation()
        {
            var recordType = new TestRecordType();
            var resp = recordType.ToTableResponse("table1");

            var json = JsonNormalizer.Serialize(resp);

            var jsonExpected = """
{
  "name": "table1",
  "schema": {
    "items": {
      "properties": {
        "field1": {
          "title": "Display1",
          "type": "string",
          "x-ms-capabilities": {
            "filterFunctions": [
              "eq"
            ]
          },
          "x-ms-sort": "asc,desc"
        },
        "field2": {
          "title": "Display2",
          "type": "string",
          "x-ms-capabilities": {
            "filterFunctions": [
              "eq"
            ]
          },
          "x-ms-sort": "asc,desc"
        }
      },
      "type": "object"
    },
    "type": "array"
  },
  "x-ms-capabilities": {
    "filterFunctionSupport": [
      "eq"
    ],
    "isOnlyServerPagable": false,
    "odataVersion": 3
  },
  "x-ms-permission": "read-write"
}
""";
            Assert.Equal(jsonExpected, json);
        }
    }

    /// <summary>
    /// Test implementation of RecordType for unit testing delegation info.
    /// </summary>
    public class TestRecordType : RecordType
    {
        private static DisplayNameProvider GetDNP()
        {
            return DisplayNameUtility.MakeUnique(new Dictionary<string, string>()
            {
                { "field1", "Display1" },
                { "field2", "Display2" }
            });
        }

        private static IReadOnlyDictionary<string, FormulaType> _fieldTypes = new Dictionary<string, FormulaType>
        {
            { "field1", FormulaType.String },
            { "field2", FormulaType.String }
        };

        private static TableDelegationInfo GetDelegationInfo()
        {
            var info = new TestDelegationInfo
            {
                DatasetName = "testDataset",
                TableName = "testTable",
                FilterSupportedFunctions = new DelegationOperator[] { DelegationOperator.Eq }
            };

            return info;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestRecordType"/> class.
        /// </summary>
        public TestRecordType()
            : base(GetDNP(), GetDelegationInfo())
        {
        }

        /// <inheritdoc/>
        public override IEnumerable<string> FieldNames => _fieldTypes.Keys;

        /// <inheritdoc/>
        public override bool TryGetFieldType(string name, out FormulaType type)
        {
            var found = _fieldTypes.TryGetValue(name, out type);
            return found;
        }

        /// <inheritdoc/>
        public override bool Equals(object other)
        {
#pragma warning disable CA1065 // Do not raise exceptions in unexpected locations
            throw new NotImplementedException();
#pragma warning restore CA1065 // Do not raise exceptions in unexpected locations
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
#pragma warning disable CA1065 // Do not raise exceptions in unexpected locations
            throw new NotImplementedException();
#pragma warning restore CA1065 // Do not raise exceptions in unexpected locations
        }
    }

    /// <summary>
    /// Test implementation of TableDelegationInfo for unit testing column capabilities.
    /// </summary>
    public class TestDelegationInfo : TableDelegationInfo
    {
        /// <inheritdoc/>
        public override bool IsDelegable => true;

        /// <inheritdoc/>
        public override ColumnCapabilitiesDefinition GetColumnCapability(string fieldName)
        {
            return new ColumnCapabilitiesDefinition
            {
                FilterFunctions = new DelegationOperator[]
                {
                    DelegationOperator.Eq
                }
            };
        }
    }
}
