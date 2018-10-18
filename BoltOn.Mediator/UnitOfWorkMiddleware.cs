using System;
using BoltOn.UoW;

namespace BoltOn.Mediator
{
	public interface IEnableUnitOfWorkMediatorMiddleware
	{
	}

	public class UnitOfWorkMiddleware : IMediatorMiddleware
	{
		private readonly IUnitOfWorkProvider _unitOfWorkProvider;

		public UnitOfWorkMiddleware(IUnitOfWorkProvider unitOfWorkProvider)
		{
			_unitOfWorkProvider = unitOfWorkProvider;
		}

		public StandardDtoReponse<TResponse> Run<TRequest, TResponse>(IRequest<TResponse> request,
																	  Func<IRequest<TResponse>, StandardDtoReponse<TResponse>> next)
			where TRequest : IRequest<TResponse>
		{
			if (!(request is IEnableUnitOfWorkMediatorMiddleware))
				return next.Invoke(request);

			var unitOfWork = _unitOfWorkProvider.Get(System.Transactions.IsolationLevel.ReadUncommitted);
			unitOfWork.Begin();
			var response = next.Invoke(request);
			unitOfWork.Commit();
			return response;
		}

		public void Dispose()
		{
		}
	}
}
