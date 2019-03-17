Mediator is the backbone of the BoltOn package. It can be used in any type of .NET application.

Create a request class by implementing `IQuery<PongResponse>` (requests can be created by implementing other interfaces too)

    public class PingRequest : IQuery<PongResponse>
	{
	}

Request, Response and RequestHandler
-----------------------------------
To create a request class, implement any of these interfaces: 

* `IRequest`
<br /> To create a request that doesn't have any response and doesn't require unit of work
* `IRequest<out TResponse>` 
<br /> To create a request with response of type TResponse and doesn't require unit of work
* `ICommand`
<br /> To create a request that doesn't have any response and that requires unit of work. A transaction with isolation level ReadCommitted will be started for the requests that implement this interface. 
* `ICommand<out TResponse>` 
<br /> To create a request with response of type TResponse and that requires require unit of work. A transaction with isolation level ReadCommitted will be started for the requests that implement this interface.
* `IQuery<out TResponse>`
<br /> To create a request with response of type TResponse and that requires unit of work. A transaction with isolation level ReadCommitted will be started for the requests that implement this interface. 
<br /> If **BoltOn.Mediator.Data.EF** is installed and bolted, DbContexts' ChangeTracker.QueryTrackingBehavior will be set to `QueryTrackingBehavior.NoTracking` and ChangeTracker.AutoDetectChangesEnabled will be set to false.
* `IStaleQuery<out TResponse>` 
<br /> To create a request with response of type TResponse and that requires require unit of work. A transaction with isolation level ReadUncommitted will be started for the requests that implement this interface.

**Note:** You could modify the transaction's default isolation level and time out by implementing `IUnitOfWorkOptionsBuilder` or by inheriting `UnitOfWorkOptionsBuilder` and overriding Build method.


