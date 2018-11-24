using System;
using BoltOn.Context;
using BoltOn.UoW;
using Microsoft.Extensions.Options;
using System.Transactions;
using BoltOn.Logging;

namespace BoltOn.Mediator
{
	public interface IEnableUnitOfWorkMiddleware
	{
	}

	public class UnitOfWorkMiddleware : BaseRequestSpecificMiddleware<IEnableUnitOfWorkMiddleware>
	{
		private readonly IUnitOfWorkProvider _unitOfWorkProvider;
		private IUnitOfWork _unitOfWork;
		private IOptions<UnitOfWorkOptions> _unitOfWorkOptions;
		private readonly IBoltOnLogger<UnitOfWorkMiddleware> _boltOnLogger;

		public UnitOfWorkMiddleware(IUnitOfWorkProvider unitOfWorkProvider,
									IOptions<UnitOfWorkOptions> unitOfWorkOptions, IBoltOnLogger<UnitOfWorkMiddleware> boltOnLogger)
		{
			_unitOfWorkProvider = unitOfWorkProvider;
			_unitOfWorkOptions = unitOfWorkOptions;
			_boltOnLogger = boltOnLogger;
		}

		public override StandardDtoReponse<TResponse> Execute<TRequest, TResponse>(IRequest<TResponse> request,
																				   Func<IRequest<TResponse>, StandardDtoReponse<TResponse>> next)
		{
			var uowOptions = _unitOfWorkOptions.Value;
			IsolationLevel isolationLevel;
			switch (request)
			{
				case ICommand<TResponse> c:
					_boltOnLogger.Debug("Getting isolation level for Command");
					isolationLevel = uowOptions.DefaultCommandIsolationLevel;
					break;
				case IQuery<TResponse> q:
					_boltOnLogger.Debug("Getting isolation level for Query");
					isolationLevel = uowOptions.DefaultQueryIsolationLevel;
					break;
				default:
					_boltOnLogger.Debug("Getting defaul isolation level");
					isolationLevel = uowOptions.DefaultIsolationLevel;
					break;
			}
			_unitOfWork = _unitOfWorkProvider.Get(isolationLevel, uowOptions.DefaultTransactionTimeout);
			_boltOnLogger.Debug($"About to begin UoW with IsolationLevel: {isolationLevel.ToString()}");
			_unitOfWork.Begin();
			var response = next.Invoke(request);
			_unitOfWork.Commit();
			_boltOnLogger.Debug("Committed UoW");
			return response;
		}

		public override void Dispose()
		{
			_unitOfWork?.Dispose();
		}
	}
}
