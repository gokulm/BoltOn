using System;
using BoltOn.UoW;
using BoltOn.Logging;
using BoltOn.Mediator.Pipeline;
using System.Threading.Tasks;
using System.Threading;

namespace BoltOn.Mediator.Interceptors
{
	public interface IEnableUnitOfWorkInterceptor
	{
	}

	public class UnitOfWorkInterceptor : BaseRequestSpecificInterceptor<IEnableUnitOfWorkInterceptor>
	{
		private readonly IUnitOfWorkManager _unitOfWorkManager;
		private IUnitOfWork _unitOfWork;
		private readonly IBoltOnLogger<UnitOfWorkInterceptor> _logger;
		private readonly IUnitOfWorkOptionsBuilder _uowOptionsBuilder;

		public UnitOfWorkInterceptor(IBoltOnLogger<UnitOfWorkInterceptor> logger,
									IUnitOfWorkManager unitOfWorkManager,
									IUnitOfWorkOptionsBuilder uowOptionsBuilder)
		{
			_unitOfWorkManager = unitOfWorkManager;
			_logger = logger;
			_uowOptionsBuilder = uowOptionsBuilder;
		}

		public async override Task<TResponse> ExecuteAsync<TRequest, TResponse>(IRequest<TResponse> request, 
			CancellationToken cancellationToken,
			Func<IRequest<TResponse>, CancellationToken, Task<TResponse>> next)
		{
			_logger.Debug($"UnitOfWorkInterceptor started");
			var unitOfWorkOptions = _uowOptionsBuilder.Build(request);
			_logger.Debug($"About to start UoW with IsolationLevel: {unitOfWorkOptions.IsolationLevel.ToString()}");
			TResponse response;
			using (_unitOfWork = _unitOfWorkManager.Get(unitOfWorkOptions))
			{
				response = await next.Invoke(request, cancellationToken);
				_unitOfWork.Commit();
			}
			_unitOfWork = null;
			_logger.Debug($"UnitOfWorkInterceptor ended");
			return response;
		}

		public override void Dispose()
		{
			_unitOfWork?.Dispose();
		}
	}
}
