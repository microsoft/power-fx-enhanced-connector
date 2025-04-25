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
    public class DelegationInfoTests
    {
        // Non-delegated example.
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

        public TestRecordType()
            : base(GetDNP(), GetDelegationInfo())
        {
        }

        public override IEnumerable<string> FieldNames => _fieldTypes.Keys;

        public override bool TryGetFieldType(string name, out FormulaType type)
        {
            var found = _fieldTypes.TryGetValue(name, out type);
            return found;
        }

        public override bool Equals(object other)
        {
#pragma warning disable CA1065 // Do not raise exceptions in unexpected locations
            throw new NotImplementedException();
#pragma warning restore CA1065 // Do not raise exceptions in unexpected locations
        }

        public override int GetHashCode()
        {
#pragma warning disable CA1065 // Do not raise exceptions in unexpected locations
            throw new NotImplementedException();
#pragma warning restore CA1065 // Do not raise exceptions in unexpected locations
        }
    }

    public class TestDelegationInfo : TableDelegationInfo
    {
        public override bool IsDelegable => true;

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
