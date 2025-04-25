# Tabular Connector

This is a Web API that implements the Power Platform Tabular Connector protocol (CDP). 
This is primarily an OData endpoint with metadata. It receives ODAta requests and translates them 
into calls to the underlying SaaS endpoint. 

The project is layered into:

1. The front-end: The actual Web API implementing the REST protocol. 
2. The back-end: Connects to the datasource. This describes the table via Power Fx interfaces.


The front-end receives has a [ITableProviderFactory](./Services/ITableProvider.cs) which will:
- take the incoming auth token 
- decode it into a property bag that's used to establish a client for the Saas. 
- translates betweent the CDP protocol and the underlying client. 

## Testing 






