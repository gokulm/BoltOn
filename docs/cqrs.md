Command Query Responsibility Segregation (CQRS)
-----------------------------------------------
The [CQRS](https://martinfowler.com/bliki/CQRS.html) implementation in this framework is based on [this article](https://jimmybogard.com/life-beyond-transactions-implementation-primer/).

The CQRS implementation will help you in using one database for writes and another database for reads, and still keep the data in sync. It can be within a same database, with writes involving foreign-key constraints and joins, and reads completely flattened out tables.

In order to implement CQRS, you need to do the following:

* Install **BoltOn.Data.EF** (soon support for CosmosDb will be added) NuGet package.
* Install **BoltOn.Bus.MassTransit** NuGet package.
* Refer to [Data](../data) and [Bus](../bus) documentation to enable the corresponding modules.
* Enable CQRS by calling BoltOnCqrsModule() in BoltOn() method.
