# Power Fx Repl with Tabular Connectors

This launches a Power Fx REPL and uses the connector SDK  to connect to a Tabular Connector instance. 

By default, it will connect to the localhost instance. 

## To run

1. First launch the CdpSampleWebApi tabular connector. This should spin up on localhost at https://localhost:7157
1. Then launch the ReplTest console app. This will initialize a Power Fx engine and include the connector at https://localhost:7157.
1. You can then enter Power Fx repl commands against the connector, like `First(table)`, Filter, CountRows, etc. 



