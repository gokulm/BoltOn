Mediator is the backbone of BoltOn. It follows the [Request/Response](https://www.enterpriseintegrationpatterns.com/patterns/messaging/RequestReply.html) and [Command Message](https://www.enterpriseintegrationpatterns.com/patterns/messaging/CommandMessage.html) patterns. It was developed after getting inspired by [Agatha](https://github.com/davybrion/Agatha), [Brighter](https://github.com/BrighterCommand/Brighter) and [MediatR](https://github.com/jbogard/MediatR).

Request, Response and RequestHandler
------------------------------------

In order to use it, you need to create a request by implementing any of these interfaces:

* `IRequest`
<br /> To create a request that doesn't have any response and doesn't require unit of work.
* `IRequest<out TResponse>` 
<br /> To create a request with response of type TResponse and doesn't require unit of work.
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

The response can be a value or a reference type, which is always tied to the request.

After declaring the request and the response, you need to create a handler by implementiong any of these interfaces:

* `IRequestHandler<in TRequest>` or `IRequestAsyncHandler<in TRequest>`
<br> For handlers that do not return any response.
* `IRequestHandler<in TRequest, TResponse>` or `IRequestAsyncHandler<in TRequest, TResponse>`
<br> For handlers that have responses.

Interceptors
------------
Every request flows thru a set of built-in interceptors, which can be controlled by implementing appropriate marker interfaces. You can create an interceptor by implementing `IInterceptor` interface.

* `StopwatchInterceptor`
<br> This interceptor logs the time that a request enters and exits the pipeline, only if the request implements `IEnableStopwatchInterceptor` interface.

* `UnitOfWorkInterceptor`
<br> This interceptor starts a transaction with an isolation level decided based on the interface (like IQuery or ICommand etc., mentioned above) that the request implements and only if it implements `IEnableUnitOfWorkInterceptor`