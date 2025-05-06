# CDP Tabular Connector Samples 

This provides a sample of how to write a CDP Tabular Connector. 

Documentation for protocol is here:
https://aka.ms/PowerFxCDP

- CdpSampleWebAPI - a REST ASP to impl CDP. Runs as an ASP.Net WebAPI. 

- CdpValidator - a client-side API that can be pointed at a CDP endpoint to validate it. This can point to existing connectors (hosted in APIM) or to localhost (such as locally running CdpSampleWebAPI)


## CdpSampleWebAPI
This is implemented on top of Power Fx definitions, notably `RecordType`. These are C# classes that already describe tabular operations: 

https://github.com/microsoft/Power-Fx/blob/main/src/libraries/Microsoft.PowerFx.Core/Public/Types/RecordType.cs

You can clone this repo, and override `ITableProvider` to provide your datasource. This project will then plumb between `RecordType` and CDP protocol. 

## Using CdpValidator

This is a command line tool which is using the following options
  - *-auth* \<path to Json file\>
  - *-logdir* \<path to log directory\>

When run from Visual Studio, those parameters can be set in Properties\launchSettings.json file

For running the validation with an APIM/CDP connector

``` JSON
{
  "endpoint":      "<APIM endpoint>",
  "environmentId": "<Environment Id>",
  "connectionId":  "<Connection Id>",
  "urlprefix":     "/apim/<Connector name>/{connectionId}",
  "dataset":       "<Dataset>",
  "tablename":     "<Table>",
  "jwtFile":       "<Path to JWT txt file>"
}
```

Here is an example

``` JSON
{
  "endpoint":      "49970107-0806-e5a7-be5e-7c60e2750f01.12.common.firstrelease.azure-apihub.net",
  "environmentId": "49970107-0806-e5a7-be5e-7c60e2750f01",
  "connectionId":  "4ac3586f00be475a8fcd09e65128c9ca",
  "urlprefix":     "/apim/sharepointonline/{connectionId}",
  "dataset":       "https://microsoft.sharepoint.com/sites/MySite",
  "tablename":     "Documents",
  "jwtFile":       "C:\\temp\\jwt.txt"
}
```

For selecting multiple datasets, you can use a star.<br>
For selecting multiple tables, you can use a star.

## What CdpValidator checks

By default, CDP validator will define a glocal timeout of 15 minutes for the entire set of checks.
This is not configurable on the command line but can be modified in Program.cs file.


Initial checks
- Get dataset metadata (/$metadata.json/datasets)
- List of datasets (/datasets)

For each dataset (except if the dataset is specified in the config file, where only one dataset will be tested)
- Get list of tables (/datasets/{datasetName}/tables)

For each table in the dataset[s] (except if the table is specified in the config file, where only onle table will be tested)
- Get table schema (/$metadata.json/datasets/{datasetName}/tables/{tableName})
- Validate each field type (cannot be Untyped Object or missing/undefined)
- Retrieve one row with $top and verify we only get one row (or zero)
- Retrieve 100 rows with $top and verify we get at most 100 rows
- Retrieve 5000 rows with $top and expect we will get less than 2100 rows (we expect paging to take place)

For tables having 2 or more columns, following tests are executed otherwise a warning is generated (and those tests skipped)
- Retrieve 10 rows (with $top) and selecting 1 column ($select)<br>
The column that is used is the first having a String or Number or Decimal or Guid or DateTime or Boolean type
- Retrieve 10 rows (with $top) and selecting 2 columns ($select), using the same column selection logic<br>

For tables with 5 or more rows, following tests are executed otherwise a warning is generated (and those tests skipped)
- Retrieve 10 rows using $filter with 1 field, equal operator (eq) and the value of the 1st row for that column
- Retrieve 10 rows using $filter with 2 fields, equal operator (eq) and the values of the 1st row for these columns
- Try retrieving 10 rows with an invalid $filter syntax ($filter=<space>)
- Retrieve 10 rows using $orderby with one column
- Retrieve 10 rows using $orderby with 3 columns

Finally
- Try getting the schema of an invalid table name (random Guid) and expecting a 400 or 404 error




## Error report

In the log folder, you'll find
- one YAML file per network call containing the request, response status code and content if any
- an Errors.http file (which can be opened in VS code) and reporting all failues<br>
These failures can be replayed for troubleshooting
