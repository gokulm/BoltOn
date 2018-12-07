using System;
using BoltOn.UoW;
using BoltOn.Logging;
using BoltOn.Mediator.Pipeline;
using BoltOn.Mediator.UoW;

namespace BoltOn.Mediator.Middlewares
{
	public interface IEnableUnitOfWorkMiddleware
	{
	}

	public class UnitOfWorkMiddleware : BaseRequestSpecificMiddleware<IEnableUnitOfWorkMiddleware>
	{
		private readonly IUnitOfWorkManager _unitOfWorkProvider;
		private IUnitOfWork _unitOfWork;
		private readonly IBoltOnLogger<UnitOfWorkMiddleware> _logger;
		private readonly IUnitOfWorkOptionsBuilder _uowOptionsBuilder;

		public UnitOfWorkMiddleware(IBoltOnLogger<UnitOfWorkMiddleware> logger, 
		                            IUnitOfWorkManager unitOfWorkProvider,
		                            IUnitOfWorkOptionsBuilder uowOptionsBuilder)
		{
			_unitOfWorkProvider = unitOfWorkProvider;
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
			using (_unitOfWork = _unitOfWorkProvider.Get(unitOfWorkOptions))
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
