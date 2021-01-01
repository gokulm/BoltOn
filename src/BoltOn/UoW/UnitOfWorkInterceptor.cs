using System;
using BoltOn.UoW;
using BoltOn.Logging;
using BoltOn.Requestor.Pipeline;
using System.Threading.Tasks;
using System.Threading;
using System.Transactions;
using BoltOn.Requestor.Interceptors;

namespace BoltOn.UoW
{
	public class UnitOfWorkInterceptor : IInterceptor
	{
		private readonly IBoltOnLogger<UnitOfWorkInterceptor> _logger;

		public UnitOfWorkInterceptor(IBoltOnLogger<UnitOfWorkInterceptor> logger)
		{
			_logger = logger;
		}

		public async Task<TResponse> RunAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken,
			Func<TRequest, CancellationToken, Task<TResponse>> next) where TRequest : IRequest<TResponse>
		{
			if (request is IEnableUnitOfWork uowRequest)
			{
				_logger.Debug($"Transaction started");
				using var transactionScope = new TransactionScope(uowRequest.TransactionScopeOption, new TransactionOptions
				{
					IsolationLevel = uowRequest.IsolationLevel,
					Timeout = uowRequest.TransactionTimeout
				}, TransactionScopeAsyncFlowOption.Enabled);
				var response = await next.Invoke(request, cancellationToken);
				transactionScope.Complete();
				_logger.Debug($"Transaction completed");
				return response;
			}
			else
			{
				_logger.Debug($"Request didn't enable UoW");
				return await next.Invoke(request, cancellationToken);
			}
		}

		public void Dispose()
		{
		}
	}
}
