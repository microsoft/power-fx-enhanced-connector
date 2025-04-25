This describes classes returned over the wire. 
The definition of these classes is via the CDP protocol specification.

All protocol classes should be Json serializable. They can use System.Text.Json attributes (which can be necessary for names like `x-ms-....`), but shouldn't need any custom converters. 

Naming convention is:

- A top-level object returned in the response has a `Response` suffix. 
- other objects have a `Poco` suffix emphasing they're pocos. 

$$$ - TBD if these are get moved into Microsoft.PowerFx.Connector nugets. 


