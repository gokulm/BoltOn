using System;
using BoltOn.UoW;
using BoltOn.Logging;

namespace BoltOn.Mediator.Middlewares
{
	public interface IEnableUnitOfWorkMiddleware
	{
	}

	public class UnitOfWorkMiddleware : BaseRequestSpecificMiddleware<IEnableUnitOfWorkMiddleware>
	{
		private readonly IUnitOfWorkProvider _unitOfWorkProvider;
		private IUnitOfWork _unitOfWork;
		private readonly IBoltOnLogger<UnitOfWorkMiddleware> _logger;
		private readonly IUnitOfWorkOptionsRetriever _unitOfWorkOptionsRetriever;

		public UnitOfWorkMiddleware(IBoltOnLogger<UnitOfWorkMiddleware> logger, 
		                            IUnitOfWorkProvider unitOfWorkProvider,
		                            IUnitOfWorkOptionsRetriever unitOfWorkOptionsRetriever)
		{
			_unitOfWorkProvider = unitOfWorkProvider;
			_logger = logger;
			_unitOfWorkOptionsRetriever = unitOfWorkOptionsRetriever;
		}

		public override MediatorResponse<TResponse> Execute<TRequest, TResponse>(IRequest<TResponse> request,
																				   Func<IRequest<TResponse>, MediatorResponse<TResponse>> next)
		{
			var unitOfWorkOptions = _unitOfWorkOptionsRetriever.Get(request);
			_unitOfWork = _unitOfWorkProvider.Get(unitOfWorkOptions.IsolationLevel, unitOfWorkOptions.TransactionTimeout);
			_logger.Debug($"About to begin UoW with IsolationLevel: {unitOfWorkOptions.IsolationLevel.ToString()}");
			_unitOfWork.Begin();
			var response = next.Invoke(request);
			_unitOfWork.Commit();
			_logger.Debug("Committed UoW");
			return response;
		}

		public override void Dispose()
		{
			_unitOfWork?.Dispose();
		}
	}
}
