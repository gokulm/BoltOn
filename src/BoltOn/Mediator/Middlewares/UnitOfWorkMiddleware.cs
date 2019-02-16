using System;
using BoltOn.UoW;
using BoltOn.Logging;
using BoltOn.Mediator.Pipeline;

namespace BoltOn.Mediator.Middlewares
{
	public interface IEnableUnitOfWorkMiddleware
	{
	}

	public class UnitOfWorkMiddleware : BaseRequestSpecificMiddleware<IEnableUnitOfWorkMiddleware>
	{
		private readonly IUnitOfWorkManager _unitOfWorkManager;
		private IUnitOfWork _unitOfWork;
		private readonly IBoltOnLogger<UnitOfWorkMiddleware> _logger;
		private readonly IUnitOfWorkOptionsBuilder _uowOptionsBuilder;

		public UnitOfWorkMiddleware(IBoltOnLogger<UnitOfWorkMiddleware> logger, 
		                            IUnitOfWorkManager unitOfWorkManager,
		                            IUnitOfWorkOptionsBuilder uowOptionsBuilder)
		{
			_unitOfWorkManager = unitOfWorkManager;
			_logger = logger;
			_uowOptionsBuilder = uowOptionsBuilder;
		}

		public override MediatorResponse<TResponse> Execute<TRequest, TResponse>(IRequest<TResponse> request,
																				   Func<IRequest<TResponse>, MediatorResponse<TResponse>> next)
		{
			_logger.Debug($"UnitOfWorkMiddleware started");
			var unitOfWorkOptions = _uowOptionsBuilder.Build(request);
			_logger.Debug($"About to start UoW with IsolationLevel: {unitOfWorkOptions.IsolationLevel.ToString()}");
			MediatorResponse<TResponse> response;
			using (_unitOfWork = _unitOfWorkManager.Get(unitOfWorkOptions))
			{
				 response = next.Invoke(request);
				_unitOfWork.Commit();
			}
			_unitOfWork = null;
			_logger.Debug($"UnitOfWorkMiddleware ended");
			return response;
		}

		public override void Dispose()
		{
			_unitOfWork?.Dispose();
		}
	}
}
