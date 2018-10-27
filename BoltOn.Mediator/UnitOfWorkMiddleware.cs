using System;
using BoltOn.UoW;

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

		public UnitOfWorkMiddleware(IUnitOfWorkProvider unitOfWorkProvider, IContextRetriever contextRetriever)
		{
			_unitOfWorkProvider = unitOfWorkProvider;
			_contextRetriever = contextRetriever;
		}

		public override StandardDtoReponse<TResponse> Execute<TRequest, TResponse>(IRequest<TResponse> request, Func<IRequest<TResponse>, StandardDtoReponse<TResponse>> next)
		{
			var mediatorContext = _contextRetriever.Get<MediatorContext>(ContextScope.App);
			System.Transactions.IsolationLevel isolationLevel;
			switch (request)
			{
				case ICommand<TResponse> c:
					isolationLevel = mediatorContext.DefaultCommandIsolationLevel;
					break;
				case IQuery<TResponse> q:
					isolationLevel = mediatorContext.DefaultQueryIsolationLevel;
					break;
				default:
					isolationLevel = mediatorContext.DefaultIsolationLevel;
					break;
			}

			_unitOfWork = _unitOfWorkProvider.Get(isolationLevel, mediatorContext.DefaultTransactionTimeout);
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
