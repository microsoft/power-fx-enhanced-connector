// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.PowerFx;
using Microsoft.PowerFx.Connectors;
using Microsoft.PowerFx.Core.Utils;
using Microsoft.PowerFx.Dataverse.Eval.Delegation.QueryExpression;
using Microsoft.PowerFx.Types;
using Microsoft.Xrm.Sdk.Query;
using static CdpValidator.Extensions;

namespace CdpValidator
{
    [SuppressMessage("Design", "CA1001: Types that own disposable fields should be disposable", Justification = "Managed internally")]
    public class ValidationWorker
    {
        private readonly List<ValidationError> _errors = new List<ValidationError>();

        private readonly HttpClient _httpClient;

        private readonly string _urlPrefix;

        private static readonly ConnectorLogger _logger = ConsoleLogger.Instance;

        private CdpInternalConnection Connection => _args.Connection;

        private Args _args;
        private readonly CdpRoot _root;
        private DatasetMetadata _datasetMetadata;
        private List<CdpDataSource> _datasets;
        private List<CdpTable> _tables;

        // Writer for .http file
        private StreamWriter _sw;
        private bool _hasError;

        private string UriPathAndQuery => RequestUri?.PathAndQuery;

        private IEnumerable<RequestLog> RequestLog => _args._httpHandler._requestLog;

        private object RequestLock => _args._httpHandler._lock;

        private Uri RequestUri
        {
            get
            {
                lock (RequestLock)
                {
                    if (RequestLog.Count() == 1)
                    {
                        return RequestLog.First().Uri;
                    }

                    var rl = RequestLog.FirstOrDefault(r => r.StatusCode != HttpStatusCode.OK);

                    return rl.Uri ?? RequestLog.First().Uri;
                }
            }
        }

        private Uri LastRequestUri
        {
            get
            {
                lock (RequestLock)
                {
                    return RequestLog.Last().Uri;
                }
            }
        }

        private HttpMethod RequestMethod
        {
            get
            {
                lock (RequestLock)
                {
                    if (RequestLog.Count() == 1)
                    {
                        return RequestLog.First().Method;
                    }

                    var rl = RequestLog.FirstOrDefault(r => r.StatusCode != HttpStatusCode.OK);

                    return rl.Method ?? RequestLog.First().Method;
                }
            }
        }

        private HttpMethod LastRequestMethod
        {
            get
            {
                lock (RequestLock)
                {
                    return RequestLog.Last().Method;
                }
            }
        }

        private Dictionary<string, string> RequestHeaders
        {
            get
            {
                lock (RequestLock)
                {
                    if (RequestLog.Count() == 1)
                    {
                        return RequestLog.First().Headers;
                    }

                    var rl = RequestLog.FirstOrDefault(r => r.StatusCode != HttpStatusCode.OK);

                    return rl.Headers ?? RequestLog.First().Headers;
                }
            }
        }

        private Dictionary<string, string> LastRequestHeaders
        {
            get
            {
                lock (RequestLock)
                {
                    return RequestLog.Last().Headers;
                }
            }
        }

        public ValidationWorker(Args args)
        {
            _args = args;
            _httpClient = args.HttpClient;
            _urlPrefix = args.UrlPrefix;
            _root = new CdpRoot(_httpClient, _urlPrefix);
            _hasError = false;
        }

        // Scan all all tables across all datasets .
        public async Task Validate(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                // Create .http file for errors   
                string httpFile = Path.Combine(_args.LogDir, "Errors.http");
                _sw = new StreamWriter(httpFile, false, Encoding.UTF8);
                WriteToHttpFile($"// CDP validation started on {DateTime.UtcNow.ToString("O")}");
                WriteToHttpFile(string.Empty);

                LogProgress($"Errors will be reported in {httpFile}");

                // Run validation
#pragma warning disable CS0618 // Type or member is obsolete (preview)
                await ValidateInternal(cancellationToken).ConfigureAwait(false);
#pragma warning restore CS0618 // Type or member is obsolete
            }
            finally
            {
                _sw.Close();
                _sw = null;
            }
        }

        private void WriteToHttpFile(string text)
        {
            const int maxLen = 180;
            string txt = text.Replace("\r", string.Empty, StringComparison.InvariantCulture).Replace("\n", string.Empty, StringComparison.InvariantCulture);

            // Split long comments on multiple lines if needed
            if (txt.Length > maxLen && txt.StartsWith("// ", StringComparison.InvariantCulture))
            {
                txt = txt.Substring(3);

                for (int i = 0; i < txt.Length; i += maxLen - 3)
                {
                    _sw.WriteLineAsync($"// {txt.Substring(i, Math.Min(177, txt.Length - i))}").ConfigureAwait(false).GetAwaiter().GetResult();
                }
            }
            else
            {
                _sw.WriteLineAsync(txt).ConfigureAwait(false).GetAwaiter().GetResult();
            }

            _sw.FlushAsync(CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        private void LogException(string source, Exception e)
        {
            if (_hasError)
            {
                WriteToHttpFile(string.Empty);
                WriteToHttpFile("###");
                WriteToHttpFile(string.Empty);
            }

            // Add error to list
            AddError(source, UriPathAndQuery, e);

            // Log to .http file
            WriteToHttpFile($"// {source} failed with exception {e.GetType().Name} - {e.Message}");
            WriteToHttpFile($"{RequestMethod} {RequestUri}");

            foreach (KeyValuePair<string, string> header in RequestHeaders)
            {
                WriteToHttpFile($"{header.Key}: {header.Value}");
            }

            _hasError = true;
        }

        private void LogError(string msg, string err)
        {
            if (_hasError)
            {
                WriteToHttpFile(string.Empty);
                WriteToHttpFile("###");
                WriteToHttpFile(string.Empty);
            }

            // Add error to list
            AddError(msg, UriPathAndQuery, err);

            // Log to .http file
            WriteToHttpFile($"// {msg} - Error {err}");
            WriteToHttpFile($"{RequestMethod} {RequestUri}");

            foreach (KeyValuePair<string, string> header in RequestHeaders)
            {
                WriteToHttpFile($"{header.Key}: {header.Value}");
            }

            _hasError = true;
        }

        private void LogErrors(List<ValidationError> errors)
        {
            if (_hasError)
            {
                WriteToHttpFile(string.Empty);
                WriteToHttpFile("###");
                WriteToHttpFile(string.Empty);
            }

            foreach (ValidationError validationError in errors)
            {
                // Log error itself 
                AddError(validationError);

                // Log to .http file the list of errors in comments
                if (validationError.Exception != null)
                {
                    WriteToHttpFile($"// {validationError.Message} failed with Exception: {validationError.Exception.GetType().Name}: {validationError.Exception.Message}");
                }
                else if (validationError.Category == ValidationCategory.Error)
                {
                    WriteToHttpFile($"// {validationError.Message} failed with Error: {validationError.Details}");
                }
                else if (validationError.Category == ValidationCategory.Warning)
                {
                    WriteToHttpFile($"// {validationError.Message} Warning: {validationError.Details}");
                }
                else
                {
                    throw new NotImplementedException("Unknown error category");
                }
            }

            // Write URL and headers to .http file
            lock (RequestLock)
            {
                if (RequestLog.Count() == 1)
                {
                    WriteToHttpFile($"{RequestMethod} {RequestUri}");

                    foreach (KeyValuePair<string, string> header in RequestHeaders)
                    {
                        WriteToHttpFile($"{header.Key}: {header.Value}");
                    }
                }
                else
                {
                    WriteToHttpFile($"{LastRequestMethod} {LastRequestUri}");

                    foreach (KeyValuePair<string, string> header in LastRequestHeaders)
                    {
                        WriteToHttpFile($"{header.Key}: {header.Value}");
                    }
                }
            }

            _hasError = true;
        }

        private void ResetLog()
        {
            _args._httpHandler.ResetLog();
        }

        [Obsolete("preview")]
        private async Task ValidateInternal(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Get dataset Metadata (these metadata are independent from the selected dataset)
            // URL = /$metadata.json/datasets
            LogProgress("Getting metadata...");
            _datasetMetadata = await GetDatasetMetadataAsync(cancellationToken).ConfigureAwait(false);

            // Get datasets
            // URL = /datasets
            LogProgress("Getting datasets...");
            ResetLog();
            IEnumerable<CdpDataSource> datasets = await GetDataSets(cancellationToken).ConfigureAwait(false);

            Console.WriteLine($"Found {datasets.Count()} datasets - {string.Join(", ", datasets.Select(ds => ds.DatasetName))}");

            if (Connection.dataset == "*")
            {
                _datasets = datasets.ToList();
            }
            else
            {
                // Use defined dataset in arguments
                CdpDataSource dataset = datasets.FirstOrDefault(ds => ds.DatasetName == Connection.dataset);

                if (dataset == null)
                {
                    LogError("No dataset", $"Cannot identify dataset '{Connection.dataset}' in list of datasets {string.Join(", ", datasets.Select(ds => ds.DatasetName))}");
                    Console.WriteLine($"   Will continue with dataset {Connection.dataset}");
                    dataset = new CdpDataSource(Connection.dataset);
                }

                _datasets = new List<CdpDataSource> { dataset };
            }

            foreach (var currentdataset in _datasets)
            {
                Console.WriteLine();
                LogProgress($"----- Current Dataset: {currentdataset.DatasetName} -----");

                // Get tables (for a given dataset)
                // URL /datasets/{datasetName}/tables
                // Note that there is no "Get table" API in CDP, only "Get tables" which enumerates all tables
                LogProgress("Getting tables...");
                ResetLog();
                IEnumerable<CdpTable> tables = await GetTablesAsync(currentdataset, cancellationToken).ConfigureAwait(false);

                Console.WriteLine($"Found {tables.Count()} tables - {string.Join(", ", tables.Select(ts => ts.TableName))}");

                if (Connection.tablename == "*")
                {
                    _tables = tables.ToList();
                }
                else
                {
                    CdpTable table = tables.FirstOrDefault(t => t.TableName == Connection.tablename || t.DisplayName == Connection.tablename);

                    if (table == null)
                    {
                        LogError("Get tables", $"Cannot identify table '{Connection.tablename}' in list of tables {string.Join(", ", tables.Select(ds => $"'{ds.TableName}' - '{ds.DisplayName}'"))}");
                        continue;
                    }

                    _tables = new List<CdpTable>() { table };
                }

                foreach (CdpTable currenttable in _tables)
                {
                    try
                    {
                        LogProgress($"----- Current Table: {currenttable.TableName} -----");

                        bool shouldContinue = true;
                        List<ValidationError> tableErrors = new List<ValidationError>();

                        try
                        {
                            // Get Table Schema
                            // /$metadata.json/datasets/{datasetName}/tables/{tableName}
                            LogProgress("Getting table schema...");
                            ResetLog();
                            await currenttable.InitAsync(_args.HttpClient, _args.UrlPrefix, cancellationToken).ConfigureAwait(false);
                        }
                        catch (PowerFxConnectorException e)
                        {
                            tableErrors.AddException(UriPathAndQuery, "Init table", e);
                            shouldContinue = false;
                        }

                        if (!currenttable.IsInitialized)
                        {
                            tableErrors.AddError(UriPathAndQuery, "Cannot initialize table", "Table not initialized");
                        }

                        if (currenttable.ConnnectorType?.HasErrors == true)
                        {
                            foreach (string err in currenttable.ConnnectorType.Errors)
                            {
                                tableErrors.AddError(UriPathAndQuery, "Invalid CdpTable Type", err);
                            }
                        }

                        if (currenttable.ConnnectorType?.HasWarnings == true)
                        {
                            foreach (ExpressionError err in currenttable.ConnnectorType.Warnings)
                            {
                                tableErrors.AddWarning(UriPathAndQuery, "CdpTable Type", err.Message);
                            }
                        }

                        if (!shouldContinue)
                        {
                            LogErrors(tableErrors);
                            continue;
                        }

                        CdpTableValue tableValue = currenttable.GetTableValue();

                        foreach (string fieldName in tableValue.RecordType.FieldNames)
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            FormulaType ft = tableValue.RecordType.GetFieldType(fieldName);

                            if (ft == null)
                            {
                                tableErrors.AddError(UriPathAndQuery, "Missing type", $"Field {Display(fieldName)} has no type");
                            }

                            if (ft == FormulaType.UntypedObject)
                            {
                                tableErrors.AddError(UriPathAndQuery, "Invalid type", $"Field {Display(fieldName)} is of type Untyped Object which isn't supported");
                            }
                        }

                        if (tableErrors.Any())
                        {
                            LogErrors(tableErrors);
                        }

                        // Read items
                        // URL = /datasets/{datasetName}/tables/{tableName}/items

                        RecordType rt = currenttable.RecordType;
                        Context context = new Context() { Currenttable = currenttable, TableErrors = tableErrors, TableValue = tableValue, CancellationToken = cancellationToken };

                        // Try getting 1 row, for testing $top
                        await ValidateGetItems("Getting items (1 row)...", "Getting 1 row", n => $"Returned too many rows: {n}", GetDelegationParam(rt, 1), n => n > 1, false, context).ConfigureAwait(false);

                        // Try getting 100 rows, for testing $top
                        IEnumerable<RecordValue> rows = await ValidateGetItems("Getting items (100 row)...", "Getting 100 row", n => $"Returned too many rows: {n}", GetDelegationParam(rt, 100), n => n > 100, false, context).ConfigureAwait(false);
                        int nRows = rows?.Count() ?? 0;

                        // Try getting 5000 rows, for making sure paging is in place (we expect less than 2100 items)
                        await ValidateGetItems("Getting items (5000 row)...", "Getting 5000 row", n => $"Returned too many rows: {n} - missing paging, expecting less than 2100", GetDelegationParam(rt, 5000), n => n > 2100, false, context).ConfigureAwait(false);

                        // Cannot try 0 or -1 row as our SDK will not emit $top in these cases

                        IEnumerable<NamedFormulaType> fields = currenttable.RecordType.GetFieldTypes();
                        int nFields = fields.Count(f => ShouldConsiderType(f.Type));

                        // We need at least 2 fields to test $select
                        if (nFields >= 2)
                        {
                            // Try getting 10 rows with only 1 column selected
                            await ValidateGetItems("Getting items (select 1 column, 10 items)...", "Get items (select 1 column, 10 items)", n => $"Returned too many rows: {n}", GetDelegationParam(rt, 10, fields, 1), n => n > 10, false, context).ConfigureAwait(false);

                            // Try getting 10 rows with only 2 columns selected
                            await ValidateGetItems("Getting items (select 2 columns, 10 items)...", "Get items (select 2 columns, 10 items)", n => $"Returned too many rows: {n}", GetDelegationParam(rt, 10, fields, 2), n => n > 10, false, context).ConfigureAwait(false);
                        }
                        else if (nFields < 2)
                        {
                            AddWarning($"Cannot test $select", $"table {currenttable.TableName}", $"Only {nFields} columns we can use in table");
                        }

                        // We need a few rows to test $filter
                        if (nRows >= 5)
                        {
                            // $filter with one column and using a value that we know is existing
                            await ValidateGetItems("Getting items (filter 'eq' 1 column)...", "Filter 'eq'", n => $"Returned too many rows: {n}", GetDelegationParam(rt, 10, fields, 0, rows, 1), n => n > 10, false, context).ConfigureAwait(false);

                            // $filter with two columns and using a value that we know is existing
                            await ValidateGetItems("Getting items (filter 'eq' 2 columns)...", "Filter 'eq'", n => $"Returned too many rows: {n}", GetDelegationParam(rt, 10, fields, 0, rows, 2), n => n > 10, false, context).ConfigureAwait(false);

                            // Invalid "$filter= " option - should return 400
                            await ValidateGetItems("Getting items ($filter= )...", "Invalid '$filter= '", n => $"Returned too many rows: {n}", GetDelegationParam(rt, 10, fields, 0, " "), n => n > 10, true, context).ConfigureAwait(false);

                            // $orderby with one column 
                            await ValidateGetItems("Getting items (order by 1 column)...", "Order by", n => $"Returned too many rows: {n}", GetDelegationParam(rt, 100, fields, 0, nOrderBy: 1), n => n > 100, false, context).ConfigureAwait(false);

                            // $orderby with 3 columns
                            await ValidateGetItems("Getting items (order by 3 columns)...", "Order by", n => $"Returned too many rows: {n}", GetDelegationParam(rt, 100, fields, 0, nOrderBy: 3), n => n > 100, false, context).ConfigureAwait(false);
                        }
                        else if (nRows < 5)
                        {
                            AddWarning($"Cannot test $filter/$orderby", $"table {currenttable.TableName}", $"{(nRows > 0 ? "Only " : string.Empty)}{nRows} rows in table");
                        }
                    }
                    catch (Exception ex)
                    {
                        LogException(string.Empty, ex);
                    }
                } // foreach table

                string invalidTableName = $"UnknownTable_{Guid.NewGuid()}";

                try
                {
                    // Try with an invalid table, should get 400 or 404
                    LogProgress("Getting invalid table...");
                    ResetLog();
                    await currentdataset.GetTableAsync(_args.HttpClient, _args.UrlPrefix, Guid.NewGuid().ToString("n"), cancellationToken).ConfigureAwait(false);
                }
                catch (PowerFxConnectorException e)
                {
                    string expected1 = "failed with 400 error";
                    string expected2 = "failed with 404 error";
                    string message = e.Message;

                    if (!message.Contains(expected1, StringComparison.OrdinalIgnoreCase) && !message.Contains(expected2, StringComparison.OrdinalIgnoreCase))
                    {
                        LogException($"Expecting 400 or 404 error for trying to initialize a random table name but got {message}", e);
                        return;
                    }
                }
            } // foreach dataset
        }

        [SuppressMessage("SpacingRules", "SA1010: Opening Square Brackets Must Be Spaced Correctly", Justification = "Not valid here")]
        [Obsolete("preview")]
        private ValidationDelegationParameters GetDelegationParam(RecordType recordType, int top, IEnumerable<NamedFormulaType> fields = null, int nSelect = 0, IEnumerable<RecordValue> rows = null, int nFilter = 0, int nOrderBy = 0)
        {
            IEnumerable<NamedFormulaType> selectedColumns = fields?.Where(nft => ShouldConsiderType(nft.Type));

            return new ValidationDelegationParameters(recordType)
            {
                Top = top,
                FxFilter = GetFilter(selectedColumns?.ToArray(), rows, nFilter),
                Columns = selectedColumns == null ? null : [.. selectedColumns.Take(nSelect).Select(nft => nft.Name.Value)],
                OrderBy = GetOrderBy(selectedColumns?.ToArray(), nOrderBy)
            };
        }

        [SuppressMessage("SpacingRules", "SA1010: Opening Square Brackets Must Be Spaced Correctly", Justification = "Not valid here")]
        [Obsolete("preview")]
        private ValidationDelegationParameters GetDelegationParam(RecordType recordType, int top, IEnumerable<NamedFormulaType> fields, int nSelect, string rawFilter)
        {
            IEnumerable<NamedFormulaType> selectedColumns = fields?.Where(nft => ShouldConsiderType(nft.Type));

            return new ValidationDelegationParameters(recordType)
            {
                Top = top,
                RawFilter = rawFilter,
                Columns = selectedColumns == null ? null : [.. selectedColumns.Take(nSelect).Select(nft => nft.Name.Value)]
            };
        }

        [Obsolete("preview")]
        private FxFilterExpression GetFilter(NamedFormulaType[] fields, IEnumerable<RecordValue> rows, int nFilter)
        {
            FxFilterExpression filter = new FxFilterExpression(FxFilterOperator.And);

            if (fields == null || nFilter == 0 || !rows.Any())
            {
                return filter;
            }

            RecordValue rv = rows.First();

            for (int i = 0; i < nFilter; i++)
            {
                string fieldName = fields[i].Name;
                FormulaValue fv = rv.GetField(fieldName);

                object val = null;
                if (fv is not BlankValue)
                {
                    val = fv.Type.GetType().Name switch
                    {
                        "StringType" => ((StringValue)fv).Value,
                        "DecimalType" => ((DecimalValue)fv).Value,
                        "NumberType" => ((NumberValue)fv).Value,

                        // $$$ Add more types
                        _ => throw new NotImplementedException($"Unknown type {fv.Type.GetType().Name}")
                    };
                }

                filter.AddCondition(new FxConditionExpression(fieldName, FxConditionOperator.Equal, val));
            }

            return filter;
        }

        [Obsolete("preview")]
        private IList<OrderExpression> GetOrderBy(NamedFormulaType[] fields, int nOrderBy)
        {
            if (nOrderBy == 0)
            {
                return null;
            }

            List<OrderExpression> orderBy = new List<OrderExpression>();
            for (int i = 0; i < nOrderBy; i++)
            {
                string fieldName = fields[i].Name;

                // Insert at the beginning of the list so that the ordered result will vary with the number of columns
                orderBy.Insert(0, new OrderExpression(fieldName, OrderType.Ascending));
            }

            return orderBy;
        }

        private static bool ShouldConsiderType(FormulaType ft)
        {
            return FormulaType.String.Equals(ft) ||
                   FormulaType.Number.Equals(ft) ||
                   FormulaType.Decimal.Equals(ft) ||
                   FormulaType.Guid.Equals(ft) ||
                   FormulaType.DateTime.Equals(ft);
        }

        private async Task<IEnumerable<RecordValue>> ValidateGetItems(string progressMessage, string message, Func<int, string> getError, ValidationDelegationParameters delegationParams, Func<int, bool> test, bool expectError, Context context)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            try
            {
                LogProgress(progressMessage);
                ResetLog();

                Stopwatch sw = new Stopwatch();
                sw.Start();

                IEnumerable<DValue<RecordValue>> items = await context.TableValue.GetRowsAsync(null, delegationParams, context.CancellationToken).ConfigureAwait(false);
                sw.Stop();

                int n = items.Count();
                context.TableErrors.Clear();

                Console.WriteLine($"Retrieved {n} rows, in {sw.ElapsedMilliseconds} ms");

                // note - there could be 0 item
                if (test(n))
                {
                    context.TableErrors.AddError(UriPathAndQuery, message, getError(n));
                }

                ValidateRows(context.Currenttable, items, delegationParams, message, context.TableErrors);

                if (context.TableErrors.Any())
                {
                    LogErrors(context.TableErrors);
                }

                return items.Where(it => !it.IsError).Select(it => it.Value);
            }
            catch (PowerFxConnectorException ex)
            {
                if (expectError)
                {
                    // Do we get the right error message?
                    if (ex.Message.Contains("failed with 400 error", StringComparison.OrdinalIgnoreCase))
                    {
                        return null;
                    }

                    LogException($"Expecting 'failed with 400 error' but received a different message - PowerFxConnectorException Exception: {ex.Message}", ex);
                }

                LogException(message, ex);

                return null;
            }
            catch (Exception ex)
            {
                LogException(message, ex);
            }

            return null;
        }

        // URL = /$metadata.json/datasets
        public async Task<DatasetMetadata> GetDatasetMetadataAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                ResetLog();
                DatasetMetadata datasetMetadata = await _root.GetDatasetMetadataAsync(cancellationToken).ConfigureAwait(false);

                // datasetMetadata.DatasetFormat can be null                
                // datasetMetadata.Parameters can be null

                if (!datasetMetadata.IsDoubleEncoding)
                {
                    LogError("Get dataset metadata", "DatasetMetadata DoubleEncoding is disabled");
                }

                if (datasetMetadata.Tabular == null)
                {
                    LogError("Get dataset metadata", "DatasetMetadata Tabular should be defined for CDP connectors");
                }
                else
                {
                    if (datasetMetadata.Tabular.Source != "mru")
                    {
                        // Could also be 'list' or 'singleton' but we don't support them yet
                        LogError("Get dataset metadata", $"DatasetMetadata.Tabular Source should be 'mru' instead of {Display(datasetMetadata.Tabular.Source)}");
                    }

                    if (datasetMetadata.Tabular.UrlEncoding != "double")
                    {
                        LogError("Get dataset metadata", $"DatasetMetadata Tabular UrlEncoding is not enabled ({Display(datasetMetadata.Tabular.UrlEncoding)})");
                    }

                    // datasetMetadata.Tabular.TableDisplayName isn't used
                    // datasetMetadata.Tabular.TablePluralName isn't used
                    // datasetMetadata.Tabular.DisplayName isn't used
                }

                return datasetMetadata;
            }
            catch (Exception e)
            {
                LogException(nameof(GetDatasetMetadataAsync), e);
                return null;
            }
        }

        public async Task<IEnumerable<CdpTable>> GetTablesAsync(CdpDataSource dataset, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            ResetLog();
            IEnumerable<CdpTable> tables = await _root.GetTablesAsync(dataset, cancellationToken).ConfigureAwait(false);
            List<ValidationError> errors = new List<ValidationError>();

            IEnumerable<CdpTable> validTables = ValidateLogicalAndDisplayNames(
                tables.Select((t, i) => new LogicalAndDisplayName<CdpTable>()
                {
                    LogicalName = t.TableName,
                    DisplayName = t.DisplayName,
                    Operation = $"Get tables - Tables[{i}]",
                    Object = t
                }),
                errors);

            if (errors.Any())
            {
                LogErrors(errors);
            }

            return tables;
        }

        public void EnsureError(string op, CdpException ex, HttpStatusCode expectedError)
        {
            var uri = ex.Request.RequestUri.ToString();
            var error = ex.ErrorResponse;
            if (error == null)
            {
                AddError(op, uri, $"Failed to deserialize body as ErrorResponse.");
            }

            if (ex.Response.StatusCode != expectedError)
            {
                AddError(op, uri, $"Wrong status code in Req. Expected {(int)expectedError}. Got {(int)ex.Response.StatusCode}");
            }

            if (error.statusCode != (int)expectedError)
            {
                AddError(op, uri, $"Wrong status code in {nameof(ErrorResponse.statusCode)} body. Expected {(int)expectedError}. Got {error.statusCode}");
            }

            if (string.IsNullOrWhiteSpace(error.message))
            {
                AddError(op, uri, $"Error response missing 'message'");
            }
        }

        // URL = /datasets
        // Returns only valid datasets
        public async Task<IReadOnlyCollection<CdpDataSource>> GetDataSets(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var list = new List<CdpDataSource>();
            var uri = _urlPrefix + "/datasets";

            try
            {
                ResetLog();
                IReadOnlyCollection<DatasetResponse.Item> datasets = await _root.GetDatasetsAsync(cancellationToken).ConfigureAwait(false);
                List<ValidationError> errors = new List<ValidationError>();

                foreach (DatasetResponse.Item validItem in ValidateLogicalAndDisplayNames(
                    datasets.Select((item, i) => new LogicalAndDisplayName<DatasetResponse.Item>()
                    {
                        LogicalName = item.Name,
                        DisplayName = item.DisplayName,
                        Operation = $"Get datasets - Datasets[{i}]",
                        Object = item
                    }),
                    errors))
                {
                    list.Add(new CdpDataSource(validItem.Name));
                }

                if (errors.Any())
                {
                    LogErrors(errors);
                }
            }
            catch (Exception ex)
            {
                LogException("Get datasets", ex);

                // fall through and return empty list.
            }

            return list;
        }

        internal class LogicalAndDisplayName<T>
        {
            public string LogicalName;
            public string DisplayName;
            public string Operation;
            public T Object;
        }

        private IReadOnlyCollection<T> ValidateLogicalAndDisplayNames<T>(IEnumerable<LogicalAndDisplayName<T>> logicalAndDisplayNames, List<ValidationError> errors)
        {
            // Sanity check...
            HashSet<string> logicalNames = new HashSet<string>();
            HashSet<string> displayNames = new HashSet<string>();
            List<T> validObjects = new List<T>();

            foreach (LogicalAndDisplayName<T> item in logicalAndDisplayNames)
            {
                if (string.IsNullOrWhiteSpace(item.LogicalName))
                {
                    errors.AddError(LastRequestUri.ToString(), item.Operation, "Name is blank (check casing)");
                }
                else
                {
                    CheckName(item.Operation, item.LogicalName, errors);

                    if (!logicalNames.Add(item.LogicalName))
                    {
                        errors.AddError(LastRequestUri.ToString(), item.Operation, $"Duplicate logical names: {item.LogicalName}");
                    }
                    else
                    {
                        // Only consider dataset if name is ok.  
                        validObjects.Add(item.Object);
                    }
                }

                if (string.IsNullOrWhiteSpace(item.DisplayName))
                {
                    errors.AddError(LastRequestUri.ToString(), item.Operation, "DisplayName is blank (check casing)");
                }
                else
                {
                    if (!displayNames.Add(item.DisplayName))
                    {
                        errors.AddError(LastRequestUri.ToString(), item.Operation, $"Duplicate display names: {item.DisplayName} (logical names={string.Join(", ", logicalAndDisplayNames.Where(it => it.DisplayName == item.DisplayName).Select(it => it.LogicalName))})");
                    }
                }
            }

            return validObjects;
        }

        private void ValidateRows(CdpTable currenttable, IEnumerable<DValue<RecordValue>> items, ValidationDelegationParameters delegParams, string message, List<ValidationError> tableErrors)
        {
#pragma warning disable CS0618 // Type or member is obsolete (preview)
            FxFilterExpression ffe = delegParams.FxFilter;
            IList<OrderExpression> ob = delegParams.OrderBy;
            FormulaValue previousValue = null;

            int count = 0;
            foreach (DValue<RecordValue> item in items)
            {
                RecordValue rv = item.Value;

                if (item.IsError)
                {
                    tableErrors.AddError(UriPathAndQuery, message, $"Item[{count}] error: {string.Join(", ", item.Error.Errors.Select(er => er.Message))}");
                }
                else if (!rv.Type.Equals(currenttable.RecordType))
                {
                    tableErrors.AddError(UriPathAndQuery, message, $"Item[{count}] - Row type does not match expected type from the table");
                }

                if (ffe != null)
                {
                    foreach (FxConditionExpression cond in ffe.Conditions)
                    {
                        string attr = cond.AttributeName;
                        FxConditionOperator op = cond.Operator;
                        object val = cond.Values.FirstOrDefault();

                        if (op == FxConditionOperator.Equal)
                        {
                            FormulaValue fv = item.Value.GetField(attr);
                            object recordValue = fv.ToObject();

                            if (!object.Equals(recordValue, val))
                            {
                                tableErrors.AddError(UriPathAndQuery, message, $"Item[{count}] - Attribute {attr} value {recordValue} doesn't match equality condition with value {val}");
                            }
                        }
                        else
                        {
                            throw new NotImplementedException("Only 'eq' conditions are supported for now");
                        }
                    }
                }

                count++;

                if (ob != null)
                {
                    // We only check with first 'order by' attribute and assume it's ascending only
                    string fieldName = ob.First().AttributeName;
                    FormulaValue currentValue = item.Value.GetField(fieldName);

                    if (previousValue == null || previousValue is BlankValue)
                    {
                        previousValue = currentValue;
                        continue;
                    }

                    if (currentValue is not BlankValue)
                    {
                        if (currentValue is StringValue sv)
                        {
                            string previous = ((StringValue)previousValue).Value;

                            if (string.Compare(previous, sv.Value, StringComparison.OrdinalIgnoreCase) > 0)
                            {
                                tableErrors.AddError(UriPathAndQuery, message, $"Item[{count}] - Attribute {fieldName} value {previous} is not ordered properly ({sv.Value} is lower)");
                            }
                        }
                        else if (currentValue is DecimalValue dv)
                        {
                            decimal previous = ((DecimalValue)previousValue).Value;

                            if (previous > dv.Value)
                            {
                                tableErrors.AddError(UriPathAndQuery, message, $"Item[{count}] - Attribute {fieldName} value {previous} is not ordered properly ({dv.Value} is lower)");
                            }
                        } 
                        else if (currentValue is NumberValue nv)
                        {
                            double previous = ((NumberValue)previousValue).Value;

                            if (previous > nv.Value)
                            {
                                tableErrors.AddError(UriPathAndQuery, message, $"Item[{count}] - Attribute {fieldName} value {previous} is not ordered properly ({nv.Value} is lower)");
                            }
                        }
                        else
                        {
                            throw new NotImplementedException($"Type {currentValue.GetType().Name} not supported");
                        }
                    }

                    previousValue = currentValue;
                }                
            }
#pragma warning restore CS0618 // Type or member is obsolete
        }

        internal static Regex _regexLogicalName = new Regex(@"^[a-zA-Z0-9_\-\\\/\.\:\[\]\{\}]{2,72}$", RegexOptions.Compiled);

        private void CheckName(string op, string name, List<ValidationError> errors)
        {
            if (!new DName(name).IsValid)
            {
                errors.AddError(UriPathAndQuery, op, $"Name {name} is not a valid Power Fx name");
            }

            if (!_regexLogicalName.IsMatch(name))
            {
                errors.AddError(UriPathAndQuery, op, $"Name {name} is not valid: {_regexLogicalName}");
            }
        }

        private void AddError(string operation, string uri, string details)
        {
            AddError(new ValidationError
            {
                Uri = uri,
                Message = operation,
                Details = details,
                Category = ValidationCategory.Error
            });
        }

        private void AddWarning(string operation, string uri, string details)
        {
            AddError(new ValidationError
            {
                Uri = uri,
                Message = operation,
                Details = details,
                Category = ValidationCategory.Warning
            });
        }

        private void AddError(string operation, string uri, Exception ex)
        {
            AddError(new ValidationError
            {
                Uri = uri,
                Message = operation,
                Details = ex.Message,
                Category = ValidationCategory.Error
            });
        }

        private void AddError(ValidationError error)
        {
            Console.ForegroundColor = error.Category == ValidationCategory.Error ? ConsoleColor.Red : ConsoleColor.DarkGray;
            Console.WriteLine($"{error.Message}: {error.Uri}");
            Console.WriteLine($"   {error.Details}");
            Console.ResetColor();

            if (error.Category == ValidationCategory.Error)
            {
                this._errors.Add(error);
            }
        }

        private void LogProgress(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        internal class Context()
        {
            public CdpTable Currenttable;
            public List<ValidationError> TableErrors;
            public CdpTableValue TableValue;
            public CancellationToken CancellationToken;
        }
    }
}
