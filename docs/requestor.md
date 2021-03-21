Requestor is the backbone of BoltOn. It follows the [Request/Response](https://www.enterpriseintegrationpatterns.com/patterns/messaging/RequestReply.html) and [Command Message](https://www.enterpriseintegrationpatterns.com/patterns/messaging/CommandMessage.html) patterns. Since it doesn't depend on any particular framework like WebAPI, MVC etc., and comprises of pure C# classes, its handlers could be added to application/service layer and could be used in a simple WebAPI based application to all the way to message queuing (MassTransit) and background tasks (Hangfire) based applications. 

The main source of inspiration for the Requestor was [Agatha](https://github.com/davybrion/Agatha), and various other projects like [Brighter](https://github.com/BrighterCommand/Brighter) and [MediatR](https://github.com/jbogard/MediatR).

Request, Response and Handler
-
In order to use the Requestor, you need to create a request by implementing any of these interfaces:

* [`IRequest`](https://github.com/gokulm/BoltOn/blob/master/src/BoltOn/Requestor/Pipeline/IRequest.cs)
<br /> To create a request that doesn't have any response and doesn't require unit of work.
* [`IRequest<out TResponse>`](https://github.com/gokulm/BoltOn/blob/master/src/BoltOn/Requestor/Pipeline/IRequest.cs)
<br /> To create a request with response of type TResponse and doesn't require unit of work.

The **response** can be any value or reference type.

After declaring the request and the response, you need to create a handler by implementiong any of these interfaces:

* [`IHandler<in TRequest>`](https://github.com/gokulm/BoltOn/blob/master/src/BoltOn/Requestor/Pipeline/IHandler.cs)
<br> For handlers that do not return any response.
* [`IHandler<in TRequest, TResponse>`](https://github.com/gokulm/BoltOn/blob/master/src/BoltOn/Requestor/Pipeline/IHandler.cs)
<br> For handlers that have responses.

Example:

    public class GetAllStudentsRequest : IRequest<IEnumerable<StudentDto>>
	{
	}

	public class GetAllStudentsHandler : IHandler<GetAllStudentsRequest, IEnumerable<StudentDto>>
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

* To invoke the handler, you need to inject `IRequestor` in a class in your application, like a controller in WebAPI or a MVC application, and call `ProcessAsync` method. Check out the implemenation [Requestor](https://github.com/gokulm/BoltOn/blob/master/src/BoltOn/Requestor/Pipeline/Requestor.cs) to know the internals.

Example:

	[Route("api/[controller]")]
	public class StudentsController : Controller
	{
		private readonly IRequestor _requestor;

		public StudentsController(IRequestor requestor)
		{
			this._requestor = requestor;
		}

		[HttpGet]
		public async Task<IEnumerable<StudentDto>> Get()
		{
			var students = await _requestor.ProcessAsync(new GetAllStudentsRequest());
			return students;
		}
	}

** Note: ** Handlers get registered automatically while bootstrapping the application using services.BoltOn(); to disable the automatic handler registrations, `DisableRequestorHandlerRegistrations()` can be used. 

Like this:

	var serviceCollection = new ServiceCollection();
	serviceCollection.BoltOn(options =>
	{
		options.DisableRequestorHandlerRegistrations();
	});

Interceptors
------------
Every request flows thru a set of built-in interceptors (mentioned below), and the execution of them can be controlled by implementing appropriate marker interfaces. 

* `StopwatchInterceptor`
<br> This interceptor logs the time that a request enters and exits the pipeline. This interceptor is enabled by default as `IRequest` implements `IEnableInterceptor<StopwatchInterceptor>` interface.

* `TransactionInterceptor`
<br> This interceptor starts a transaction with an isolation level set in the request class. This interceptor is enabled only if the request implements `IEnableTransaction` interface. 

You could create an interceptor by implementing `IInterceptor` interface. If you want to control the execution of an interceptor based on the incoming request, you can make the request implement `IEnableInterceptor<TInterceptor>` and add a check something like this:

	if (!(request is IEnableInterceptor<StopwatchInterceptor>))
		return await next.Invoke(request, cancellationToken);

**Note: **

* Interceptors from all the attached modules and assemblies **must be** added explicitly  using `AddInterceptor<TInterceptor>` extension method.
* Interceptors get executed in the order they're added.
* Interceptors can be removed using `RemoveInterceptor<TInterceptor>` extension method. 
* All the interceptors in the pipeline (in other packages) can be removed using `RemoveAllInterceptors` extension method. However, if this extension method is executed in a registration task and if there is another registration task after your registration task to add interceptors, those interceptors will be added to the pipeline.
* Interceptors can be added before or after an existing interceptor using `Before<TInterceptor>` or `After<TInterceptor>` respectively.

	Example:

		bootstrapperOptions.AddInterceptor<TransactionInterceptor>().Before<StopwatchInterceptor>();

Why Requestor?
--------------
Adding a request, a handler and a response for every single request may look like as if we are writing a lot of code, but the advantages of it are:

1. Every handler class will be responsible for one and only one thing, and thus follow **Single Responsibility Principle**, which makes them **readable, maintainable, testable** and **reusable**. 
2. The same request/handler and the requestor can be used in Web API based applications, background jobs using [Hangfire](../hangfire) and message queueing using [AppServiceBus](../bus), and thus it will be easy for developers to pick it up.
3. Common functionality like logging, caching, unit of work etc., can be abstracted out and can be added to handlers easily using [interceptors](../requestor/#interceptors).  