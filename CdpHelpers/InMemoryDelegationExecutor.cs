// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.PowerFx.Core.Entities;
using Microsoft.PowerFx.Dataverse;
using Microsoft.PowerFx.Dataverse.Eval.Delegation.QueryExpression;
using Microsoft.PowerFx.Types;
using Microsoft.Xrm.Sdk.Query;

namespace CdpHelpers
{
    /// <summary>
    /// Executes in-memory delegation logic for Dataverse queries, applying filter, order, top, and projection operations.
    /// </summary>
    internal class InMemoryDelegationExecutor
    {
        /// <summary>
        /// Executes the in-memory query operations on the provided table using the specified parameters.
        /// </summary>
        /// <param name="sourceTable">The source table to query.</param>
        /// <param name="parameters">Delegation parameters including filter, order, top, and projection info.</param>
        /// <returns>An enumerable of <see cref="RecordValue"/> representing the query result.</returns>
        public static IEnumerable<RecordValue> Execute(
        TableValue sourceTable,
        DataverseDelegationParameters parameters)
        {
            // 1) Pull out all the RecordValue rows
            var rows = sourceTable
                .Rows
                .Where(dv => dv.IsValue)
                .Select(dv => dv.Value)
                .ToList();

            // 2) Apply $filter
            if (parameters.FxFilter != null)
            {
                rows = rows
                    .Where(r => EvaluateFilter(parameters.FxFilter, r))
                    .ToList();
            }

            // 3) Apply $orderby
            if (parameters.OrderBy != null && parameters.OrderBy.Any())
            {
                rows = ApplyOrdering(rows, parameters.OrderBy).ToList();
            }

            // 4) Apply $top
            if (parameters.Top.HasValue && parameters.Top.Value > 0)
            {
                rows = rows.Take(parameters.Top.Value).ToList();
            }

            // 5) Apply $select (projection)
            if (parameters.ColumnMap != null)
            {
                rows = rows
                    .Select(r => ProjectColumns(r, parameters.ColumnMap))
                    .ToList();
            }

            if ((parameters.Joins != null && parameters.Joins.Any()) || (parameters.ColumnMap?.Any(c => c.IsDistinct || c.AggregateMethod != SummarizeMethod.None) == true))
            {
                throw new NotImplementedException("Join and aggregation not implemented in in‐memory mock.");
            }

            return rows;
        }

        /// <summary>
        /// Evaluates a filter expression against a record.
        /// </summary>
        /// <param name="filter">The filter expression to evaluate.</param>
        /// <param name="record">The record to test.</param>
        /// <returns>True if the record matches the filter; otherwise, false.</returns>
        private static bool EvaluateFilter(FxFilterExpression filter, RecordValue record)
        {
            // Combine sub‐filters:
            if (filter.Filters?.Any() ?? false)
            {
                var op = filter.FilterOperator == FxFilterOperator.And
                    ? new Func<bool, bool, bool>((a, b) => a && b)
                    : new Func<bool, bool, bool>((a, b) => a || b);

                return filter.Filters
                    .Select(sub => EvaluateFilter(sub, record))
                    .Aggregate(op);
            }

            // Combine conditions:
            if (filter.Conditions?.Any() ?? false)
            {
                var op = filter.FilterOperator == FxFilterOperator.And
                    ? new Func<bool, bool, bool>((a, b) => a && b)
                    : new Func<bool, bool, bool>((a, b) => a || b);

                return filter.Conditions
                    .Select(cond => EvaluateCondition(cond, record))
                    .Aggregate(op);
            }

            // no filter means include everything
            return true;
        }

        /// <summary>
        /// Evaluates a single condition expression against a record.
        /// </summary>
        /// <param name="cond">The condition expression.</param>
        /// <param name="record">The record to test.</param>
        /// <returns>True if the record matches the condition; otherwise, false.</returns>
        private static bool EvaluateCondition(FxConditionExpression cond, RecordValue record)
        {
            // 1) Pull raw value from the record
            var fv = record.GetField(cond.AttributeName);
            object actualValue = fv switch
            {
                NumberValue nv => nv.Value,
                DecimalValue dv => dv.Value,
                StringValue sv => sv.Value,
                GuidValue gv => gv.Value,
                DateTimeValue dv => dv.Value,
                DateValue dv => dv.Value,
                TimeValue tv => tv.Value,
                BooleanValue bv => bv.Value,
                _ => throw new NotImplementedException($"Unhandled field type {fv.GetType().Name} for {cond.AttributeName}")
            };

            object compareTo = cond.Values.FirstOrDefault();

            if (actualValue is decimal && compareTo is not decimal)
            {
                compareTo = Convert.ToDecimal(compareTo, CultureInfo.InvariantCulture);
            }
            else if (compareTo is decimal && actualValue is not decimal)
            {
                actualValue = Convert.ToDouble(actualValue, CultureInfo.InvariantCulture);
            }

            // Apply any FieldFunction
            foreach (var ff in cond.FieldFunctions)
            {
                bool result = false;
                switch (ff)
                {
                    case FieldFunction.StartsWith:
                        result = ((string)actualValue)?.StartsWith((string)compareTo, StringComparison.InvariantCulture) ?? false;
                        return result;
                    case FieldFunction.EndsWith:
                        result = ((string)actualValue)?.EndsWith((string)compareTo, StringComparison.InvariantCulture) ?? false;
                        return result;
                    case FieldFunction.Year:
                        result = ((DateTime)actualValue).Year.Equals((int)compareTo);
                        return result;
                    case FieldFunction.Month:
                        result = ((DateTime)actualValue).Month.Equals((int)compareTo);
                        return result;
                    case FieldFunction.Hour:
                        result = ((DateTime)actualValue).Hour.Equals((int)compareTo);
                        return result;
                }
            }

            // Compare based on operator
            switch (cond.Operator)
            {
                case FxConditionOperator.Equal:
                    return Equals(actualValue, compareTo);

                case FxConditionOperator.NotEqual:
                    return !Equals(actualValue, compareTo);

                case FxConditionOperator.GreaterThan:
                    return ((IComparable)actualValue).CompareTo(compareTo) > 0;

                case FxConditionOperator.GreaterEqual:
                    return ((IComparable)actualValue).CompareTo(compareTo) >= 0;

                case FxConditionOperator.LessThan:
                    return ((IComparable)actualValue).CompareTo(compareTo) < 0;

                case FxConditionOperator.LessEqual:
                    return ((IComparable)actualValue).CompareTo(compareTo) <= 0;

                case FxConditionOperator.Contains:
                    return ((string)actualValue)?.Contains((string)compareTo, StringComparison.InvariantCulture) ?? false;

                case FxConditionOperator.BeginsWith:
                    return ((string)actualValue)?.StartsWith((string)cond.Values[0], StringComparison.InvariantCulture) ?? false;

                case FxConditionOperator.EndsWith:
                    return ((string)actualValue)?.EndsWith((string)cond.Values[0], StringComparison.InvariantCulture) ?? false;

                case FxConditionOperator.Null:
                    return actualValue == null;

                case FxConditionOperator.NotNull:
                    return actualValue != null;

                case FxConditionOperator.ContainsValues:
                    return cond.Values
                        .Cast<object>()
                        .Any(v => Equals(actualValue, v));

                // … handle other operators as needed …

                default:
                    throw new NotSupportedException($"Operator {cond.Operator} not implemented in in‐memory mock.");
            }
        }

        /// <summary>
        /// Applies multi-column ordering to a sequence of <see cref="RecordValue"/>.
        /// </summary>
        /// <param name="rows">The rows to order.</param>
        /// <param name="orderBy">The list of order expressions.</param>
        /// <returns>An ordered enumerable of <see cref="RecordValue"/>.</returns>
        private static IEnumerable<RecordValue> ApplyOrdering(
            IEnumerable<RecordValue> rows,
            IList<OrderExpression> orderBy)
        {
            // Nothing to do if no ordering specified
            if (orderBy == null || !orderBy.Any())
            {
                return rows;
            }

            IOrderedEnumerable<RecordValue> orderedRows = null;

            foreach (var orderExpr in orderBy)
            {
                // Build key selector for this column
                Func<RecordValue, IComparable> keySelector =
                    record => GetComparableField(record, orderExpr.AttributeName);

                // First pass: OrderBy / OrderByDescending
                if (orderedRows == null)
                {
                    orderedRows = orderExpr.OrderType == OrderType.Ascending
                        ? rows.OrderBy(keySelector)
                        : rows.OrderByDescending(keySelector);
                }
                else
                {
                    // Subsequent passes: ThenBy / ThenByDescending
                    orderedRows = orderExpr.OrderType == OrderType.Ascending
                        ? orderedRows.ThenBy(keySelector)
                        : orderedRows.ThenByDescending(keySelector);
                }
            }

            return orderedRows;
        }

        /// <summary>
        /// Extracts a comparable CLR value from a <see cref="RecordValue"/> field.
        /// </summary>
        /// <param name="record">The record containing the field.</param>
        /// <param name="fieldName">The name of the field to extract.</param>
        /// <returns>A comparable value for ordering, or null if not applicable.</returns>
        private static IComparable GetComparableField(RecordValue record, string fieldName)
        {
            var value = record.GetField(fieldName);

            return value switch
            {
                NumberValue nv => nv.Value,
                DecimalValue dv => dv.Value,
                StringValue sv => sv.Value,
                GuidValue gv => gv.Value,
                DateTimeValue dv => dv.Value,
                DateValue dv => dv.Value,
                TimeValue tv => tv.Value,
                BooleanValue bv => bv.Value,
                _ => null
            };
        }

        /// <summary>
        /// Projects a record to a new record containing only the specified columns.
        /// </summary>
        /// <param name="row">The source record.</param>
        /// <param name="columnMap">The columns to include in the projection.</param>
        /// <returns>A new <see cref="RecordValue"/> with only the selected columns.</returns>
        private static RecordValue ProjectColumns(
            RecordValue row,
            IEnumerable<FxColumnInfo> columnMap)
        {
            var resultRecordType = RecordType.Empty();
            var columnValues = new List<NamedValue>();
            foreach (var fxColumn in columnMap)
            {
                var colType = row.Type.GetFieldType(fxColumn.RealColumnName);
                resultRecordType = resultRecordType.Add(fxColumn.RealColumnName, colType, fxColumn.AliasColumnName);
                var colValue = row.GetField(fxColumn.RealColumnName);
                columnValues.Add(new NamedValue(fxColumn.RealColumnName, colValue));
            }

            var resultRecordValue = FormulaValue.NewRecordFromFields(resultRecordType, columnValues);
            return resultRecordValue;
        }
    }
}
