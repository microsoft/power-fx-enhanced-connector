// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.UriParser;
using Microsoft.PowerFx.Dataverse.Eval.Delegation.QueryExpression;

#pragma warning disable CS0618 // [Obsolete]

namespace Microsoft.PowerFx.Connectors
{
    /// <summary>
    /// Convert from OData <see cref="QueryNode"/> tree to a Power Fx delegation tree.
    /// </summary>
    public static class OData2XrmConverter
    {
        // Convert an OData filter to a Dataverse filter.
        // Alternative would be to use their vistor, QueryNodeVisitor<T>.
        public static FxFilterExpression Parse(SingleValueNode node)
        {
            if (node is BinaryOperatorNode bin)
            {
                // Is it a logical operator?
                switch (bin.OperatorKind)
                {
                    case BinaryOperatorKind.And:
                        return ParseLogical(bin, FxFilterOperator.And);

                    case BinaryOperatorKind.Or:
                        return ParseLogical(bin, FxFilterOperator.Or);
                }

                // Field op Value
                if (bin.Left is SingleValuePropertyAccessNode left)
                {
                    var leftName = left.Property.Name;

                    // field ne null
                    if (bin.Right is ConvertNode c1 && c1.Source.Kind == QueryNodeKind.Constant && c1.Source.TypeReference == null)
                    {
                        var filter = new FxFilterExpression();
                        switch (bin.OperatorKind)
                        {
                            case BinaryOperatorKind.Equal:
                                filter.AddCondition(leftName, FxConditionOperator.Null);
                                return filter;

                            case BinaryOperatorKind.NotEqual:
                                filter.AddCondition(leftName, FxConditionOperator.NotNull);
                                return filter;

                            default:
                                throw new InvalidOperationException($"Unhandled null op: {bin.OperatorKind}");
                        }
                    }

                    // field op consttant
                    if (bin.Right is ConstantNode c)
                    {
                        var op = bin.OperatorKind;
                        var xrmOp = Convert(op);

                        var cond = new FxConditionExpression(leftName, xrmOp, c.Value);

                        var filter = new FxFilterExpression();
                        filter.AddCondition(cond);
                        return filter;
                    }
                }

                throw new NotImplementedException($"Unhandled binary node.");
            }

            if (node is SingleValueNode s)
            {
                switch (s.Kind)
                {
                    // Function call returning a single value.
                    case QueryNodeKind.SingleValueFunctionCall:
                        if (node is SingleValueFunctionCallNode svfunc)
                        {
                            string funcName = svfunc.Name;
                            List<QueryNode> parameters = svfunc.Parameters.ToList();

                            var filter = new FxFilterExpression();

                            if (funcName == "contains" && parameters.Count() == 2 && parameters[0] is SingleValuePropertyAccessNode prop && parameters[1] is ConstantNode c)
                            {
                                string propName = prop.Property.Name;

                                var cond = new FxConditionExpression(propName, FxConditionOperator.Contains, c.Value);
                                filter.AddCondition(cond);
                                return filter;
                            }
                        }

                        break;
                }

                throw new NotImplementedException($"Unhandled single value node.");
            }

            throw new NotImplementedException($"Unhandled node:  {node.GetType().FullName}");
        }

        private static FxConditionOperator Convert(BinaryOperatorKind op)
        {
            switch (op)
            {
                case BinaryOperatorKind.GreaterThan: return FxConditionOperator.GreaterThan;
                case BinaryOperatorKind.LessThan: return FxConditionOperator.LessThan;
                case BinaryOperatorKind.Equal: return FxConditionOperator.Equal;
                case BinaryOperatorKind.GreaterThanOrEqual: return FxConditionOperator.GreaterEqual;
                case BinaryOperatorKind.LessThanOrEqual: return FxConditionOperator.LessEqual;
                case BinaryOperatorKind.NotEqual: return FxConditionOperator.NotEqual;
            }

            throw new NotImplementedException($"Can't convert {op}");
        }

        // Parse a binary operation that is And/OR
        private static FxFilterExpression ParseLogical(BinaryOperatorNode bin, FxFilterOperator op)
        {
            var f1 = Parse(bin.Left);
            var f2 = Parse(bin.Right);

            var filter = new FxFilterExpression(op);
            filter.Filters.Add(f1);
            filter.Filters.Add(f2);

            return filter;
        }
    }
}
