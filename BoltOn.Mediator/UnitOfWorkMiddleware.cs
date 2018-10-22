using System;
using BoltOn.UoW;

namespace BoltOn.Mediator
{
	public interface IEnableUnitOfWorkMiddleware
	{
	}

	public class UnitOfWorkMiddleware : IMediatorMiddleware
	{
		private readonly IUnitOfWorkProvider _unitOfWorkProvider;
		private readonly IContextRetriever _contextRetriever;
		private IUnitOfWork _unitOfWork;

		public UnitOfWorkMiddleware(IUnitOfWorkProvider unitOfWorkProvider, IContextRetriever contextRetriever)
		{
			_unitOfWorkProvider = unitOfWorkProvider;
			_contextRetriever = contextRetriever;
		}

		public StandardDtoReponse<TResponse> Run<TRequest, TResponse>(IRequest<TResponse> request,
																	  Func<IRequest<TResponse>, StandardDtoReponse<TResponse>> next)
			where TRequest : IRequest<TResponse>
		{
			if (!(request is IEnableUnitOfWorkMiddleware))
				return next.Invoke(request);

			var mediatorContext = _contextRetriever.Get<MediatorContext>(ContextScope.App);
			System.Transactions.IsolationLevel isolationLevel;
			switch(request)
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

			_unitOfWork = _unitOfWorkProvider.Get(isolationLevel);
			_unitOfWork.Begin();
			var response = next.Invoke(request);
			_unitOfWork.Commit();
			return response;
		}

		public void Dispose()
		{
			_unitOfWork?.Dispose();
		}
	}
}
