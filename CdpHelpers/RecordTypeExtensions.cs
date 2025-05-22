// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CdpHelpers;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Microsoft.PowerFx.Core.Entities;
using Microsoft.PowerFx.Core.Functions.Delegation;
using Microsoft.PowerFx.Dataverse;
using Microsoft.PowerFx.Dataverse.Eval.Delegation.QueryExpression;
using Microsoft.PowerFx.Types;

#pragma warning disable CS0618 // [Obsolete]

namespace Microsoft.PowerFx.Connectors
{
    public static class RecordTypeExtensions
    {
        // Convert a Power Fx record into a EdmModel and type.
        public static (EdmModel, IEdmType) GetModel(this RecordType record)
        {
            EdmModel model = new EdmModel();

            var fields = record.GetFieldTypes();
            var t1 = new EdmComplexType("Sample1", "name1");

            foreach (var field in fields)
            {
                string name = field.Name;
                string displayName = field.DisplayName;
                if (field.IsExpandEntity)
                {
                    // $$$ Deal with relationships later.
                    continue;
                }

                FormulaType type = field.Type;

                EdmPrimitiveTypeKind kind;
                if (type == FormulaType.String)
                {
                    kind = EdmPrimitiveTypeKind.String;
                }
                else if (type == FormulaType.Number || type == FormulaType.Decimal)
                {
                    // $$$ This is lossy!!!
                    kind = EdmPrimitiveTypeKind.Int32;
                }
                else
                {
                    // Ignore unrecognized types.
                    continue;
                }

                t1.AddStructuralProperty(name, kind);
            }

            model.AddElement(t1);

            return (model, t1);
        }

        // $$$ Can we build this ontop of GetModel()?
        // delegationInfo needed since each column has delegation info.
        public static TableSchemaPoco ToTableSchemaPoco(this RecordType record)
        {
            var fields = record.GetFieldTypes();

            var props = new Dictionary<string, ColumnInfoPoco>();

            foreach (var field in fields)
            {
                string name = field.Name;
                string displayName = field.DisplayName;
                if (field.IsExpandEntity)
                {
                    // $$$ Deal with relationships later.
                    continue;
                }

                FormulaType type = field.Type;

                string typeStr;
                if (type == FormulaType.String)
                {
                    typeStr = "string";
                }
                else if (type == FormulaType.Number || type == FormulaType.Decimal)
                {
                    // $$$ This is lossy!!!
                    typeStr = "integer";
                }
                else
                {
                    // Ignore unrecognized types.
                    continue;
                }

                ColumnCapabilitiesPoco capPoco = null;
                string sort = null;

                var delegationInfo = new MockTableDelegationInfo();
                var columnCapability = delegationInfo.GetColumnCapability(name);
                if (columnCapability != null)
                {
                    // $$$ Add capabilities here.
                    capPoco = ColumnCapabilitiesPoco.New(columnCapability.FilterFunctions);

                    // $$$ Get real capabilities.
                    // Need this on TableDelegationInfo
                    sort = "none";
                }

                props[name] = new ColumnInfoPoco
                {
                    Title = displayName,
                    Type = typeStr,
                    Capabilities = capPoco,
                    Sort = sort
                };
            }

            return new TableSchemaPoco
            {
                Items = new ItemsPoco
                {
                    Properties = props
                }
            };
        }

        public static GetTablePoco ToTableResponse(this RecordType record, string tableName)
        {
            // if you have record type, this could come from record.TryGetCapabilities(out var delegationInfo);
            // this can be different for different table or datasource.
            var delegationInfo = new MockTableDelegationInfo();

            var resp = new GetTablePoco
            {
                Name = tableName,
                Permissions = "read-write",
                Schema = record.ToTableSchemaPoco()
            };

            if (delegationInfo != null && delegationInfo.IsDelegable)
            {
                var c = new CapabilitiesPoco();
                resp.Capabilities = c;

                c.OdataVersion = 3;
                c.SetOps(delegationInfo.FilterSupportedFunctions);

                if (delegationInfo.SortRestriction != null)
                {
                    c.SortRestrictions = new Sort
                    {
                        Sortable = true,
                        UnsortableProperties = delegationInfo.SortRestriction.UnsortableProperties.ToArray()
                    };
                }

                if (delegationInfo.FilterRestriction != null)
                {
                    c.FilterRestrictions = new Filter
                    {
                        Filterable = true,
                        NonFilterableProperties = delegationInfo.FilterRestriction.NonFilterableProperties.ToArray()
                    };
                }
            }

            return resp;
        }

        // Convert OData query into a PowerFx delegation query object.
        public static DelegationParameters Convert(
            this RecordType record, // used to build model
            IDictionary<string, string> query)
        {
            (var model, var type) = record.GetModel();

            // var options = query.GetNamedValues().ToStrDict();

            IEdmNavigationSource source = null;
            ODataQueryOptionParser parser2 = new ODataQueryOptionParser(model, type, source, query);

            // This will throw ODataException if the field is missing from the model.
            FilterClause filter2 = parser2.ParseFilter();

            OrderByClause orderBy2 = parser2.ParseOrderBy();

            var f2 = (filter2 == null) ?
                new FxFilterExpression() :
                OData2XrmConverter.Parse(filter2.Expression);

            IList<Xrm.Sdk.Query.OrderExpression> fxOrderList = null;
            if (orderBy2 != null)
            {
                string fieldName = ((SingleValuePropertyAccessNode)orderBy2.Expression).Property.Name;
                Xrm.Sdk.Query.OrderType direction =
                    (orderBy2.Direction == OrderByDirection.Ascending) ?
                        Xrm.Sdk.Query.OrderType.Ascending :
                        Xrm.Sdk.Query.OrderType.Descending;

                var fxOrder = new Xrm.Sdk.Query.OrderExpression(fieldName, direction);
                fxOrderList = new List<Xrm.Sdk.Query.OrderExpression>
                {
                    fxOrder,
                };
            }

            FxColumnMap columnMap = null;
            if (query.TryGetValue("$select", out var selectStr))
            {
                var select = selectStr.Split(',');
                columnMap = FxColumnMap.New(select);
            }

            // $$$ change for Count, Summarize?
            FormulaType returnType = record;
            var parameters = new DataverseDelegationParameters(returnType)
            {
                FxFilter = f2,
                ColumnMap = columnMap,
                OrderBy = fxOrderList
            };

            if (query.TryGetValue("$top", out var topStr))
            {
                parameters.Top = int.Parse(topStr, NumberStyles.Integer, CultureInfo.InvariantCulture);
            }

            return parameters;
        }
    }
}
