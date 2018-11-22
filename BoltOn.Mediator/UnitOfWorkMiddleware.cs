using System;
using BoltOn.Context;
using BoltOn.UoW;
using Microsoft.Extensions.Options;
using System.Transactions;

namespace BoltOn.Mediator
{
	public interface IEnableUnitOfWorkMiddleware
	{
	}

	public class UnitOfWorkMiddleware : BaseRequestSpecificMiddleware<IEnableUnitOfWorkMiddleware>
	{
		private readonly IUnitOfWorkProvider _unitOfWorkProvider;
		private readonly IContextRetriever _contextRetriever;
		private IUnitOfWork _unitOfWork;
		private IOptions<UnitOfWorkOptions> _unitOfWorkOptions;

		public UnitOfWorkMiddleware(IUnitOfWorkProvider unitOfWorkProvider, IContextRetriever contextRetriever,
		                            IOptions<UnitOfWorkOptions> unitOfWorkOptions)
		{
			_unitOfWorkProvider = unitOfWorkProvider;
			_contextRetriever = contextRetriever;
			_unitOfWorkOptions = unitOfWorkOptions;
		}

		public override StandardDtoReponse<TResponse> Execute<TRequest, TResponse>(IRequest<TResponse> request, 
		                                                                           Func<IRequest<TResponse>, StandardDtoReponse<TResponse>> next)
		{
			var uowOptions = _unitOfWorkOptions.Value;
			IsolationLevel isolationLevel;
			switch (request)
			{
				case ICommand<TResponse> c:
					isolationLevel = uowOptions.DefaultCommandIsolationLevel;
					break;
				case IQuery<TResponse> q:
					isolationLevel = uowOptions.DefaultQueryIsolationLevel;
					break;
				default:
					isolationLevel = uowOptions.DefaultIsolationLevel;
					break;
			}
			_unitOfWork = _unitOfWorkProvider.Get(isolationLevel, uowOptions.DefaultTransactionTimeout);
			_unitOfWork.Begin();
			var response = next.Invoke(request);
			_unitOfWork.Commit();
			return response;
		}

		public override void Dispose()
		{
			_unitOfWork?.Dispose();
		}
	}
}
