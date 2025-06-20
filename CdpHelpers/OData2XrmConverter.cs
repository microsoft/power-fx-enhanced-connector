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
    /// Provides functionality to convert OData <see cref="QueryNode"/> trees to Power Fx delegation trees
    /// for use with Microsoft Dataverse. This static utility class handles the translation between 
    /// OData query syntax and Power Fx filter expressions, enabling delegation of queries to Dataverse.
    /// </summary>
    /// <remarks>
    /// This converter supports a subset of OData operations that can be efficiently delegated to Dataverse:
    /// <list type="bullet">
    /// <item><description>Binary logical operators (And, Or)</description></item>
    /// <item><description>Comparison operators (Equal, NotEqual, GreaterThan, LessThan, etc.)</description></item>
    /// <item><description>Null value checks</description></item>
    /// <item><description>String functions like 'contains'</description></item>
    /// </list>
    /// 
    /// The conversion process maintains the semantic meaning of OData queries while producing 
    /// Power Fx expressions that can be efficiently executed against Dataverse.
    /// </remarks>
    /// <seealso cref="QueryNode"/>
    /// <seealso cref="FxFilterExpression"/>    /// <seealso cref="Microsoft.PowerFx.Dataverse.Eval.Delegation.QueryExpression"/>
    public static class OData2XrmConverter
    {
        /// <summary>
        /// Converts an OData <see cref="SingleValueNode"/> filter to a Power Fx <see cref="FxFilterExpression"/> 
        /// that can be used for Dataverse delegation.
        /// </summary>
        /// <param name="node">The OData single value node to convert. Must not be null. Supports binary operators (And, Or, comparisons), 
        /// property access nodes, constant nodes, and function calls like 'contains'.</param>
        /// <returns>A <see cref="FxFilterExpression"/> representing the converted filter expression that can be 
        /// used for Power Fx delegation to Dataverse.</returns>
        /// <exception cref="InvalidOperationException">Thrown when an unsupported null operation is encountered.</exception>
        /// <exception cref="NotImplementedException">Thrown when the node type or operation is not yet supported 
        /// by the converter.</exception>
        /// <example>
        /// <code>
        /// // Convert an OData filter like "Name eq 'John' and Age gt 25"
        /// // The resulting FxFilterExpression can be used for Dataverse delegation
        /// var filterExpression = OData2XrmConverter.Parse(odataNode);
        /// </code>
        /// </example>
        /// <remarks>
        /// This method recursively parses OData query nodes and converts them to Power Fx filter expressions.
        /// Supported operations include:
        /// - Logical operators: And, Or
        /// - Comparison operators: Equal, NotEqual, GreaterThan, LessThan, GreaterThanOrEqual, LessThanOrEqual
        /// - Null checks: field eq null, field ne null
        /// - Function calls: contains(field, value)
        /// - Property access with constant values
        /// 
        /// Alternative implementation could use OData's QueryNodeVisitor&lt;T&gt; pattern.
        /// </remarks>
        /// <seealso cref="ParseLogical(BinaryOperatorNode, FxFilterOperator)"/>
        /// <seealso cref="Convert(BinaryOperatorKind)"/>
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

        /// <summary>
        /// Converts an OData <see cref="BinaryOperatorKind"/> to the corresponding Power Fx <see cref="FxConditionOperator"/>.
        /// </summary>
        /// <param name="op">The OData binary operator to convert.</param>
        /// <returns>The equivalent <see cref="FxConditionOperator"/> for use in Power Fx filter expressions.</returns>
        /// <exception cref="NotImplementedException">Thrown when the specified <paramref name="op"/> is not supported for conversion.</exception>
        /// <remarks>
        /// This method provides a mapping between OData comparison operators and their Power Fx equivalents.
        /// Only comparison operators are supported; logical operators (And, Or) are handled separately in the main Parse method.
        /// </remarks>
        private static FxConditionOperator Convert(BinaryOperatorKind op)
        {
            switch (op)
            {
                case BinaryOperatorKind.GreaterThan: return FxConditionOperator.GreaterThan;                case BinaryOperatorKind.LessThan: return FxConditionOperator.LessThan;
                case BinaryOperatorKind.Equal: return FxConditionOperator.Equal;
                case BinaryOperatorKind.GreaterThanOrEqual: return FxConditionOperator.GreaterEqual;
                case BinaryOperatorKind.LessThanOrEqual: return FxConditionOperator.LessEqual;
                case BinaryOperatorKind.NotEqual: return FxConditionOperator.NotEqual;
            }

            throw new NotImplementedException($"Can't convert {op}");
        }

        /// <summary>
        /// Parses a binary logical operation (And/Or) from an OData <see cref="BinaryOperatorNode"/> 
        /// and converts it to a Power Fx <see cref="FxFilterExpression"/>.
        /// </summary>
        /// <param name="bin">The binary operator node containing the left and right operands to be parsed.</param>
        /// <param name="op">The logical operator (And or Or) to apply between the parsed operands.</param>
        /// <returns>A <see cref="FxFilterExpression"/> containing the logical operation with both operands as child filters.</returns>
        /// <remarks>
        /// This method recursively calls the main <see cref="Parse(SingleValueNode)"/> method to convert 
        /// the left and right operands, then combines them using the specified logical operator.
        /// The resulting filter expression will have the two parsed operands as child filters.
        /// </remarks>
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
