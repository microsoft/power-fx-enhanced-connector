# Tabular Connector

This is a Web API that implements the Power Platform Tabular Connector protocol (CDP). 
This is primarily an OData endpoint with metadata. It receives ODAta requests and translates them 
into calls to the underlying SaaS endpoint. 

The project is layered into:

1. The front-end: The actual Web API implementing the REST protocol. 
2. The back-end: Connects to the datasource. This describes the table via Power Fx interfaces.


The front-end has a [ITableProviderFactory](./Services/ITableProvider.cs) which will:
- receive the settings from the `TableProvider` configuration section
- use them to establish a client for the SaaS
- translate between the CDP protocol and the underlying client.

## Provider settings

Settings for your data source, such as its endpoint and credentials, are read from the `TableProvider` configuration section and passed to `ITableProviderFactory.Get`. They aren't read from the request.

```json
"TableProvider": {
  "Endpoint": "https://<your-data-source>"
}
```

Keep secrets out of source control. Use `dotnet user-secrets` during development, and app settings or Azure Key Vault when deployed.

## Authentication

This sample doesn't authenticate callers, so you can run it locally without any setup (for example with [ReplTest](../ReplTest/readme.md) or `CdpSampleWebApi.http`).

Before you connect it to real data, add authentication that matches the authentication type of your custom connector, such as an API key or OAuth 2.0 with Microsoft Entra ID. Validate the credential in this service on every request to the CDP routes, because the service can be called directly and not only through the connector.

## Testing 

Run `dotnet test Tests\CdpSampleWebApi.Tests` from the repository root.
