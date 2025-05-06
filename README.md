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

