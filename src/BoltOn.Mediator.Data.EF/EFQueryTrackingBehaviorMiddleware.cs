using System;
using BoltOn.Logging;
using BoltOn.Mediator.Middlewares;
using BoltOn.Mediator.Pipeline;

namespace BoltOn.Mediator.Data.EF
{
	public class EFQueryTrackingBehaviorMiddleware : IMediatorMiddleware
	{
		private readonly IBoltOnLogger<EFQueryTrackingBehaviorMiddleware> _logger;
		private readonly IMediatorDataContext _mediatorDataContext;

		public EFQueryTrackingBehaviorMiddleware(IBoltOnLogger<EFQueryTrackingBehaviorMiddleware> logger,
			IMediatorDataContext mediatorDataContext)
		{
			_logger = logger;
			_mediatorDataContext = mediatorDataContext;
		}

		public MediatorResponse<TResponse> Run<TRequest, TResponse>(IRequest<TResponse> request, 
			Func<IRequest<TResponse>, MediatorResponse<TResponse>> next) where TRequest : IRequest<TResponse>
		{
			_logger.Debug($"Entering {nameof(EFQueryTrackingBehaviorMiddleware)}...");
			_mediatorDataContext.IsQueryRequest = request is IQuery<TResponse>;
			_logger.Debug($"IsQueryRequest: {_mediatorDataContext.IsQueryRequest}");
			var response = next(request);
			return response;
		}

		public void Dispose()
		{
		}
	}
}
