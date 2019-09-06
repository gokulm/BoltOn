Logging
-------
BoltOn uses .NET Core's logger internally, with just a custom adapter to help in unit testing. You could use any logging provider as you wish, or you could inherit [`BoltOnLogger<TType>`](https://github.com/gokulm/BoltOn/blob/master/src/BoltOn/Logging/BoltOnLogger.cs) and override the logging methods.

IQueryUncommitted
-----------------
In case if you want a custom request type which is completely different from IQuery or ICommand in terms of the transaction isolation level or transaction timeout, you could create one something like this:

    public interface IQueryUncommitted<out TResponse> : IRequest<TResponse>, IEnableUnitOfWorkInterceptor
    {
    }

And then create a custom interceptor in case if you want to tweak the ChangeTrackerContext, like the one mentioned [below](#interceptor) and create a custom UnitOfWorkOptionsBuilder to change the transaction isolation level or timeout, like the one mentioned [below](#unitofworkoptionsbuilder). And, finally register them like this:

    serviceCollection.RemoveInterceptor<ChangeTrackerInterceptor>();
    serviceCollection.AddInterceptor<CustomChangeTrackerInterceptor>();
    serviceCollection.AddTransient<IUnitOfWorkOptionsBuilder, CustomUnitOfWorkOptionsBuilder>();

Interceptor
-----------

You could create your custom interceptor for change tracking something like this:

    public class CustomChangeTrackerInterceptor : IInterceptor
	{
		private readonly ChangeTrackerContext _changeTrackerContext;

		public CustomChangeTrackerInterceptor(ChangeTrackerContext changeTrackerContext)
		{
			_changeTrackerContext = changeTrackerContext;
		}

		public TResponse Run<TRequest, TResponse>(IRequest<TResponse> request, 
			Func<IRequest<TResponse>, TResponse> next) where TRequest : IRequest<TResponse>
		{
			_changeTrackerContext.IsQueryRequest = request is IQuery<TResponse> || request is IQueryUncommitted<TResponse>;
			var response = next(request);
			return response;
		}

		public async Task<TResponse> RunAsync<TRequest, TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken, 
			Func<IRequest<TResponse>, CancellationToken, Task<TResponse>> next) where TRequest : IRequest<TResponse>
		{
			_changeTrackerContext.IsQueryRequest = request is IQuery<TResponse> || request is IQueryUncommitted<TResponse>;
			var response = await next(request, cancellationToken);
			return response;
		}

		public void Dispose()
		{
		}
	}

UnitOfWorkOptionsBuilder
------------------------

You could create your custom UnitOfWorkOptionsBuilder to change the isolation level and/or transaction timeout based on request type something like this:

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

