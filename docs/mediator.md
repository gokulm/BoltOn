Mediator is the backbone of BoltOn. It follows the [Request/Response](https://www.enterpriseintegrationpatterns.com/patterns/messaging/RequestReply.html) and [Command Message](https://www.enterpriseintegrationpatterns.com/patterns/messaging/CommandMessage.html) patterns. It was developed after getting inspired by [Agatha](https://github.com/davybrion/Agatha), [Brighter](https://github.com/BrighterCommand/Brighter) and [MediatR](https://github.com/jbogard/MediatR).

Request, Response and RequestHandler
------------------------------------
In order to use Mediator, you need to create a request by implementing any of these interfaces:

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

The response can be a value or a reference type, which is always tied to the request.

After declaring the request and the response, you need to create a handler by implementiong any of these interfaces:

* `IRequestHandler<in TRequest>` or `IRequestAsyncHandler<in TRequest>`
<br> For handlers that do not return any response.
* `IRequestHandler<in TRequest, TResponse>` or `IRequestAsyncHandler<in TRequest, TResponse>`
<br> For handlers that have responses.

**Note:** `MediatorRegistrationTask` takes care of registering the handers to the DI framework.

Interceptors
------------
Every request flows thru a set of built-in interceptors, which can be controlled by implementing appropriate marker interfaces. 

* `StopwatchInterceptor`
<br> This interceptor logs the time that a request enters and exits the pipeline. This interceptor is enabled only if the request implements `IEnableStopwatchInterceptor` interface.

* `UnitOfWorkInterceptor`
<br> This interceptor starts a transaction with an isolation level based on the interface like IQuery or ICommand etc., (mentioned above) that the request implements. This interceptor is enabled only if the request implements `IEnableUnitOfWorkInterceptor`

You can create an interceptor by implementing `IInterceptor` interface. If you want to enable or disable an interceptor based on a marker interface implementation, you can inherit `BaseRequestSpecificInterceptor<T>`

Interceptors can be added and removed using the extension methods `AddInterceptor<TInterceptor>` and `RemoveInterceptor<TInterceptor>` (in BoltOn.Mediator namespace) respectively. All the interceptors can be removed using the extension method `RemoveAllInterceptors`.

Unit of Work
------------
If you're using Mediator, you need not worry about starting or committing an unit of work, it will be done automatically using `UnitOfWorkInterceptor`. In case if you're not using Mediator or you want to take control on starting an unit of work, use `IUnitOfWorkManager`. It takes care of starting a new transaction with `System.Transactions.TransactionScopeOption.RequiresNew` if there is one already started. 

**Note:** Though it's possible to start an unit of work manually, please try to do avoid it, especially when there is already one, as having more than one unit of work isn't a proper way to build applications. This will be useful only when you want to query a database with an isolation level different from the one started by `UnitOfWorkInterceptor`.

In case if you want to change the default transaction isolation level for all the requests or only certain requests, or if you want to change the TransactionTimeout, you can implement `IUnitOfWorkOptionsBuilder` or inherit `UnitOfWorkOptionsBuilder` and override Build method.

