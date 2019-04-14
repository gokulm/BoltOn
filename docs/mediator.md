Mediator is the backbone of BoltOn. It follows the [Request/Response](https://www.enterpriseintegrationpatterns.com/patterns/messaging/RequestReply.html) and [Command Message](https://www.enterpriseintegrationpatterns.com/patterns/messaging/CommandMessage.html) patterns. 

The main source of inspiration for the Mediator was [Agatha](https://github.com/davybrion/Agatha), and various other projects like [Brighter](https://github.com/BrighterCommand/Brighter) and [MediatR](https://github.com/jbogard/MediatR).

Request, Response and RequestHandler
------------------------------------
In order to use the Mediator, you need to create a request by implementing any of these interfaces:

* `IRequest`
<br /> To create a request that doesn't have any response and doesn't require unit of work.
* `IRequest<out TResponse>` 
<br /> To create a request with response of type TResponse and doesn't require unit of work.
* `ICommand`
<br /> To create a request that doesn't have any response and that requires unit of work. A transaction with isolation level **ReadCommitted** will be started for the requests that implement this interface. 
* `ICommand<out TResponse>` 
<br /> To create a request with response of type TResponse and that requires require unit of work. A transaction with isolation level **ReadCommitted** will be started for the requests that implement this interface.
* `IQuery<out TResponse>`
<br /> To create a request with response of type TResponse and that requires unit of work. A transaction with isolation level ReadCommitted will be started for the requests that implement this interface. 
<br /> If **BoltOn.Data.EF** is installed and bolted, DbContexts' ChangeTracker.QueryTrackingBehavior will be set to `QueryTrackingBehavior.NoTracking` and `ChangeTracker.AutoDetectChangesEnabled` will be set to false in [DbContextFactory](../data/#dbcontextfactory).

In case if you want a custom request type with a different isolation level, you could create an interface and customize `UnitOfWorkOptionsBuilder` by overriding it or by creating a new one. Check out this custom request type called `IQueryUncommitted` with isolation level **ReadUncommitted** [here](../optional/#iqueryuncommitted). 

The **response** can be any value or reference type.

After declaring the request and the response, you need to create a handler by implementiong any of these interfaces:

* `IRequestHandler<in TRequest>` or `IRequestAsyncHandler<in TRequest>`
<br> For handlers that do not return any response.
* `IRequestHandler<in TRequest, TResponse>` or `IRequestAsyncHandler<in TRequest, TResponse>`
<br> For handlers that have responses.

Example:

    public class GetAllStudentsRequest : IQuery<IEnumerable<StudentDto>>
	{
	}

	public class GetAllStudentsHandler : IRequestAsyncHandler<GetAllStudentsRequest, IEnumerable<StudentDto>>
	{
		public async Task<IEnumerable<StudentDto>> HandleAsync(GetAllStudentsRequest request, CancellationToken cancellationToken)
		{
			var students = new List<StudentDto>
			{
				new StudentDto { FirstName = "first", LastName = "last" }
			};
			return await Task.FromResult(students);
		}
	}

Interceptors
------------
Every request flows thru a set of built-in interceptors, which can be controlled by implementing appropriate marker interfaces. 

* `StopwatchInterceptor`
<br> This interceptor logs the time that a request enters and exits the pipeline. This interceptor is enabled only if the request implements `IEnableStopwatchInterceptor` interface.

* `UnitOfWorkInterceptor`
<br> This interceptor starts a transaction with an isolation level based on the interface like IQuery or ICommand etc., (mentioned above) that the request implements. This interceptor is enabled only if the request implements `IEnableUnitOfWorkInterceptor`

You can create an interceptor by implementing `IInterceptor` interface, like [this](../optional/#interceptor). If you want to enable or disable an interceptor based on a marker interface implementation, you can inherit `BaseRequestSpecificInterceptor<T>`

**Note: **

* Interceptors from all the bolted modules and assemblies **must be** added explicitly in the registration tasks.
* Interceptors can be added and removed using the extension methods `AddInterceptor<TInterceptor>` and `RemoveInterceptor<TInterceptor>` respectively. 
* Interceptors added using `AddInterceptor<TInterceptor>` will be executed in the order they're added.
* In case if you want to remove all the interceptors in the pipeline, use the extension method `RemoveAllInterceptors`. However, if this extension method is executed in a registration task and if there is another registration task after your registration task with `AddInterceptor<TInterceptor>` call, those interceptors will be added to the pipeline.

Unit of Work
------------

* If you use Mediator and implement any of the interfaces like IQuery or ICommand, which in turn implements `IEnableUnitOfWorkInterceptor`, you need not worry about starting or committing unit of work, it will be done automatically using `UnitOfWorkInterceptor`. 
* If you're not using Mediator and if you want to start an unit of work, call Get method in `IUnitOfWorkManager` by passing `UnitOfWorkOptions` based on your needs. It will start a new transaction with `System.Transactions.TransactionScopeOption.RequiresNew` if there is one already started. The default transaction isolation level is `IsolationLevel.Serializable`

**Note:** Though it's possible to start an unit of work manually, please try to do avoid it, especially when there is already one, as having more than one unit of work isn't a proper way to build applications. This will be useful only when you want to query a database with an isolation level different from the one started by `UnitOfWorkInterceptor`.

In case if you want to change the default transaction isolation level for all the requests or only certain requests, or if you want to change the TransactionTimeout, you can implement `IUnitOfWorkOptionsBuilder` like [this](../optional/#unitofworkoptionsbuilder) or inherit `UnitOfWorkOptionsBuilder` and override the Build method.

