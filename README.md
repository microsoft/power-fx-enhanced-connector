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
