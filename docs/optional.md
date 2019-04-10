Logging
-------
BoltOn uses .NET Core's logger internally, with just a custom adapter to help in unit testing. You could use any logging provider as you wish, or you could inherit `BoltOnLogger<TType>` and override the logging methods.

Utilities
---------
* **Check.Requires**
<br>
There are instances where you have to check for a condition and throw exception if the condition fails, in those instances you could use `Check.Requires`

    Example:

        Check.Requires(_serviceCollection != null, "ServiceCollection not initialized"); 

    is equivalent to

        if(_serviceCollection == null)
            throw new Exception("ServiceCollection not initialized");

    and custom exceptions can be thrown like this:

        Check.Requires<CustomException>(_serviceCollection != null, "ServiceCollection not initialized"); 

* **IBoltOnClock/BoltOnClock**
<br>
There are instances where you have to use static properties DateTime.Now or DateTimeOffset.UtcNow, which makes hard to unit test, in those instances you could inject `IBoltOnClock`

IQueryUncommitted
-----------------
In case if you want a custom request type which is completely different from IQuery or ICommand in terms of the transaction isolation level or transaction timeout, you could create one something like this:

    public interface IQueryUncommitted<out TResponse> : IRequest<TResponse>, IEnableUnitOfWorkInterceptor
    {
    }

Create a custom interceptor in case if you want to tweak the MediatorContext, like the one mentioned [below](/optional/#interceptor) and create a custom UnitOfWorkOptionsBuilder to change the transaction isolation level or timeout, like the one mentioned [below](/optional/#unitofworkoptionsbuilder). And, finally register the custom interceptor and UnitOfWorksOptionsBuilder like this:

    serviceCollection.RemoveInterceptor<MediatorContextInterceptor>();
    serviceCollection.AddInterceptor<CustomMediatorContextInterceptor>();
    serviceCollection.AddTransient<IUnitOfWorkOptionsBuilder, CustomUnitOfWorkOptionsBuilder>();

Interceptor
-----------

    public class CustomMediatorContextInterceptor : IInterceptor
	{
		private readonly MediatorContext _mediatorContext;

		public CustomMediatorContextInterceptor(MediatorContext mediatorContext)
		{
			_mediatorContext = mediatorContext;
		}

		public TResponse Run<TRequest, TResponse>(IRequest<TResponse> request, 
			Func<IRequest<TResponse>, TResponse> next) where TRequest : IRequest<TResponse>
		{
			_mediatorContext.IsQueryRequest = request is IQuery<TResponse> || request is IQueryUncommitted<TResponse>;
			var response = next(request);
			return response;
		}

		public async Task<TResponse> RunAsync<TRequest, TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken, 
			Func<IRequest<TResponse>, CancellationToken, Task<TResponse>> next) where TRequest : IRequest<TResponse>
		{
			_mediatorContext.IsQueryRequest = request is IQuery<TResponse> || request is IQueryUncommitted<TResponse>;
			var response = await next(request, cancellationToken);
			return response;
		}

		public void Dispose()
		{
		}
	}

UnitOfWorkOptionsBuilder
------------------------

    public class CustomUnitOfWorkOptionsBuilder : IUnitOfWorkOptionsBuilder
    {
        private readonly IBoltOnLogger<CustomUnitOfWorkOptionsBuilder> _logger;

        public CustomUnitOfWorkOptionsBuilder(IBoltOnLogger<CustomUnitOfWorkOptionsBuilder> logger)
        {
            _logger = logger;
        }

        public UnitOfWorkOptions Build<TResponse>(IRequest<TResponse> request)
        {
			IsolationLevel isolationLevel;
			switch (request)
            {
                case ICommand<TResponse> _:
                case IQuery<TResponse> _:
				case ICommand _:
					_logger.Debug("Getting isolation level for Command or Query");
                    isolationLevel = IsolationLevel.ReadCommitted;
                    break;
				case IQueryUncommitted<TResponse> _:
					_logger.Debug("Getting isolation level for QueryUncommitted");
					isolationLevel = IsolationLevel.ReadUncommitted;
					break;
				default:
                    throw new Exception("Request should implement ICommand<> or IQuery<> to enable Unit of Work.");
            }
            return new UnitOfWorkOptions { IsolationLevel = isolationLevel };
        }
    }

