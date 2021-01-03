using System;
using BoltOn.Logging;
using BoltOn.Requestor.Pipeline;
using System.Threading.Tasks;
using System.Threading;
using System.Transactions;
using BoltOn.Requestor.Interceptors;

namespace BoltOn.Transaction
{
	public class TransactionInterceptor : IInterceptor
	{
		private readonly IBoltOnLogger<TransactionInterceptor> _logger;

		public TransactionInterceptor(IBoltOnLogger<TransactionInterceptor> logger)
		{
			_logger = logger;
		}

		public async Task<TResponse> RunAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken,
			Func<TRequest, CancellationToken, Task<TResponse>> next) where TRequest : IRequest<TResponse>
		{
			if (request is IEnableTransaction transactionRequest)
			{
				_logger.Debug($"About to start transaction. TransactionScopeOption: {transactionRequest.TransactionScopeOption} " +
					$"IsolationLevel: {transactionRequest.IsolationLevel} Timeout: {transactionRequest.TransactionTimeout}");
				using var transactionScope = new TransactionScope(transactionRequest.TransactionScopeOption, new TransactionOptions
				{
					IsolationLevel = transactionRequest.IsolationLevel,
					Timeout = transactionRequest.TransactionTimeout
				}, TransactionScopeAsyncFlowOption.Enabled);
				var response = await next.Invoke(request, cancellationToken);
				transactionScope.Complete();
				_logger.Debug($"Transaction completed");
				return response;
			}
			else
			{
				_logger.Debug($"Request didn't enable transaction");
				return await next.Invoke(request, cancellationToken);
			}
		}

		public void Dispose()
		{
		}
	}
}
