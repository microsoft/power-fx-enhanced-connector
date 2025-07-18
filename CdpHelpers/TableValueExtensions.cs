﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CdpHelpers;
using Microsoft.PowerFx.Dataverse;
using Microsoft.PowerFx.Types;

namespace Microsoft.PowerFx.Connectors
{
    /// <summary>
    /// Extension methods for <see cref="TableValue"/> to support OData item response generation.
    /// </summary>
    public static class TableValueExtensions
    {
        /// <summary>
        /// Converts a <see cref="TableValue"/> to a <see cref="GetItemsResponse"/> asynchronously, applying OData query parameters.
        /// </summary>
        /// <param name="tableValue">The table value to convert.</param>
        /// <param name="odataParameters">OData query parameters to apply.</param>
        /// <param name="cancellationToken">A cancellation token for the async operation.</param>
        /// <returns>A <see cref="GetItemsResponse"/> containing the resulting items.</returns>
        public static async Task<GetItemsResponse> ToGetItemsResponseAsync(this TableValue tableValue, IDictionary<string, string> odataParameters, CancellationToken cancellationToken)
        {
            IEnumerable<RecordValue> results;

            cancellationToken.ThrowIfCancellationRequested();

            var record = tableValue.Type.ToRecord();
            var parameters = record.Convert(odataParameters);

            if (tableValue is IDelegatableTableValue v2)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                // $$$ Should use ExecuteQueryAsync instead which is returning a FormulaValue
                var rows = await v2.GetRowsAsync(null, parameters, cancellationToken).ConfigureAwait(false);
                results = rows.Select(r => r.Value).ToList();
#pragma warning restore CS0618 // Type or member is obsolete
            }
            else
            {
                // Trivial case for in-memory example when we don't support IDelegatableTableValue.
                results = InMemoryDelegationExecutor.Execute(tableValue, (DataverseDelegationParameters)parameters);
            }

            var resp = new GetItemsResponse();
            resp.value = new List<Dictionary<string, object>>();

            foreach (var result in results)
            {
                var dict = result.GetODataValues();
                resp.value.Add(dict);
            }

            return resp;
        }
    }
}
