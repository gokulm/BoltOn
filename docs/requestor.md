Requestor is the backbone of BoltOn. It follows the [Request/Response](https://www.enterpriseintegrationpatterns.com/patterns/messaging/RequestReply.html) and [Command Message](https://www.enterpriseintegrationpatterns.com/patterns/messaging/CommandMessage.html) patterns. Since it doesn't depend on any particular framework like WebAPI, MVC etc., and comprises of pure C# classes, its handlers could be added to application/service layer and could be used in a simple WebAPI based application to all the way to message queuing (MassTransit) and background tasks (Hangfire) based applications. 

The main source of inspiration for the Requestor was [Agatha](https://github.com/davybrion/Agatha), and various other projects like [Brighter](https://github.com/BrighterCommand/Brighter) and [MediatR](https://github.com/jbogard/MediatR).

Request, Response and Handler
-
In order to use the Requestor, you need to create a request by implementing any of these interfaces:

* [`IRequest`](https://github.com/gokulm/BoltOn/blob/master/src/BoltOn/Requestor/IRequest.cs)
<br /> To create a request that doesn't have any response and doesn't require unit of work.
* [`IRequest<out TResponse>`](https://github.com/gokulm/BoltOn/blob/master/src/BoltOn/Requestor/IRequest.cs)
<br /> To create a request with response of type TResponse and doesn't require unit of work.

The **response** can be any value or reference type.

After declaring the request and the response, you need to create a handler by implementiong any of these interfaces:

* [`IHandler<in TRequest>`](https://github.com/gokulm/BoltOn/blob/master/src/BoltOn/Requestor/IHandler.cs)
<br> For handlers that do not return any response.
* [`IHandler<in TRequest, TResponse>`](https://github.com/gokulm/BoltOn/blob/master/src/BoltOn/Requestor/IHandler.cs)
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

* To invoke the handler, you need to inject `IRequestor` in a class in your application, like a controller in WebAPI or a MVC application, and call `ProcessAsync` method. Check out the implemenation [Requestor](https://github.com/gokulm/BoltOn/blob/master/src/BoltOn/Requestor/Requestor.cs) to know the internals.

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

Why Requestor?
--------------
Adding a request, a handler and a response for every single request may look like as if we are writing a lot of code, but the advantages of it are:

1. Every handler class will be responsible for one and only one thing, and thus follow **Single Responsibility Principle**, which makes them **readable, maintainable, testable** and **reusable**. 
2. The same request/handler and the requestor can be used in Web API based applications, background jobs using [Hangfire](../hangfire) and message queueing using [AppServiceBus](../bus), and thus it will be easy for developers to pick it up. 