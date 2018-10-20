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
		private IUnitOfWork _unitOfWork;

		public UnitOfWorkMiddleware(IUnitOfWorkProvider unitOfWorkProvider)
		{
			_unitOfWorkProvider = unitOfWorkProvider;
		}

		public StandardDtoReponse<TResponse> Run<TRequest, TResponse>(IRequest<TResponse> request,
																	  Func<IRequest<TResponse>, StandardDtoReponse<TResponse>> next)
			where TRequest : IRequest<TResponse>
		{
			if (!(request is IEnableUnitOfWorkMiddleware))
				return next.Invoke(request);

			// retrieve mediator options and set the isolation level

			System.Transactions.IsolationLevel isolationLevel;
			switch(request)
			{
				case ICommand<TResponse> c:
					isolationLevel = System.Transactions.IsolationLevel.ReadCommitted;
					break;
				case IQuery<TResponse> q:
					isolationLevel = System.Transactions.IsolationLevel.ReadUncommitted;
					break;
				default:
					isolationLevel = System.Transactions.IsolationLevel.ReadCommitted;
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
