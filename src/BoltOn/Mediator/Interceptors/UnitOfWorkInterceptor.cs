using System;
using BoltOn.UoW;
using BoltOn.Logging;
using BoltOn.Mediator.Pipeline;
using System.Threading.Tasks;
using System.Threading;

namespace BoltOn.Mediator.Interceptors
{
	public class UnitOfWorkInterceptor : IInterceptor
	{
		private readonly IUnitOfWorkManager _unitOfWorkManager;
		private IUnitOfWork _unitOfWork;
		private readonly IBoltOnLogger<UnitOfWorkInterceptor> _logger;
		private readonly IUnitOfWorkOptionsBuilder _uowOptionsBuilder;

		public UnitOfWorkInterceptor(IBoltOnLogger<UnitOfWorkInterceptor> logger,
									IUnitOfWorkManager unitOfWorkManager,
									IUnitOfWorkOptionsBuilder uowOptionsBuilder)
		{
			_unitOfWorkManager = unitOfWorkManager;
			_logger = logger;
			_uowOptionsBuilder = uowOptionsBuilder;
		}

        public async Task<TResponse> RunAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken, 
            Func<TRequest, CancellationToken, Task<TResponse>> next) where TRequest : IRequest<TResponse>
		{
			if (!(request is IEnableInterceptor<UnitOfWorkInterceptor>))
                return await next.Invoke(request, cancellationToken);

            _logger.Debug($"UnitOfWorkInterceptor started");
            var unitOfWorkOptions = _uowOptionsBuilder.Build(request);
            _logger.Debug($"About to start UoW with IsolationLevel: {unitOfWorkOptions.IsolationLevel.ToString()}");
            TResponse response;
            using (_unitOfWork = _unitOfWorkManager.Get(unitOfWorkOptions))
            {
                response = await next.Invoke(request, cancellationToken);
                _unitOfWork.Commit();
            }
            _unitOfWork = null;
            _logger.Debug($"UnitOfWorkInterceptor ended");
            return response;
		}

		public void Dispose()
		{
			_unitOfWork?.Dispose();
		}
    }
}
