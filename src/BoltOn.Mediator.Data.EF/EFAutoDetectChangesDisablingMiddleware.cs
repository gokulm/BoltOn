using System;
using BoltOn.Logging;
using BoltOn.Mediator.Middlewares;
using BoltOn.Mediator.Pipeline;

namespace BoltOn.Mediator.Data.EF
{
	public class EFAutoDetectChangesDisablingMiddleware : IMediatorMiddleware
	{
		private readonly IBoltOnLogger<EFAutoDetectChangesDisablingMiddleware> _logger;
		private readonly IMediatorDataContext _mediatorDataContext;

		public EFAutoDetectChangesDisablingMiddleware(IBoltOnLogger<EFAutoDetectChangesDisablingMiddleware> logger,
			IMediatorDataContext mediatorDataContext)
		{
			_logger = logger;
			_mediatorDataContext = mediatorDataContext;
		}

		public MediatorResponse<TResponse> Run<TRequest, TResponse>(IRequest<TResponse> request, Func<IRequest<TResponse>, MediatorResponse<TResponse>> next) where TRequest : IRequest<TResponse>
		{
			_logger.Debug($"Entering {nameof(EFAutoDetectChangesDisablingMiddleware)}...");
			_mediatorDataContext.IsAutoDetectChangesEnabled = !(request is IQuery<TResponse>);
			_logger.Debug($"IsAutoDetectChangesDisabled: {_mediatorDataContext.IsAutoDetectChangesEnabled}");
			var response = next(request);
			return response;
		}

		public void Dispose()
		{
		}
	}
}
