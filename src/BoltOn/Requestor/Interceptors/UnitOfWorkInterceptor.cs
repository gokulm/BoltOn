using System;
using BoltOn.UoW;
using BoltOn.Logging;
using BoltOn.Requestor.Pipeline;
using System.Threading.Tasks;
using System.Threading;
using System.Transactions;

namespace BoltOn.Requestor.Interceptors
{
	public class UnitOfWorkInterceptor : IInterceptor
	{
		private readonly IBoltOnLogger<UnitOfWorkInterceptor> _logger;
		private readonly IUnitOfWorkOptionsBuilder _uowOptionsBuilder;

		public UnitOfWorkInterceptor(IBoltOnLogger<UnitOfWorkInterceptor> logger,
									IUnitOfWorkOptionsBuilder uowOptionsBuilder)
		{
			_logger = logger;
			_uowOptionsBuilder = uowOptionsBuilder;
		}

        public async Task<TResponse> RunAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken, 
            Func<TRequest, CancellationToken, Task<TResponse>> next) where TRequest : IRequest<TResponse>
		{
			if (!(request is IEnableInterceptor<UnitOfWorkInterceptor>))
                return await next.Invoke(request, cancellationToken);

            _logger.Debug($"UnitOfWorkInterceptor started");
            var uowOptions = _uowOptionsBuilder.Build(request);
			using var transactionScope = new TransactionScope(uowOptions.TransactionScopeOption, new TransactionOptions
			{
				IsolationLevel = uowOptions.IsolationLevel,
				Timeout = uowOptions.TransactionTimeout
			}, TransactionScopeAsyncFlowOption.Enabled);
			var response = await next.Invoke(request, cancellationToken);
			transactionScope.Complete();
			_logger.Debug($"UnitOfWorkInterceptor ended");
			return response;
		}

		public void Dispose()
		{
		}
    }
}
