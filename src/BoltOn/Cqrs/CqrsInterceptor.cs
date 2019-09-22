using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BoltOn.Data;
using BoltOn.Logging;
using BoltOn.Mediator.Interceptors;
using BoltOn.Mediator.Pipeline;
using BoltOn.UoW;
using System.Transactions;

namespace BoltOn.Cqrs
{
	public class CqrsInterceptor : IInterceptor
	{
		private readonly EventBag _eventBag;
		private readonly IBoltOnLogger<CqrsInterceptor> _logger;
		private readonly IEventDispatcher _eventDispatcher;
		private readonly ICqrsRepositoryFactory _cqrsRepositoryFactory;
		private readonly IUnitOfWorkManager _unitOfWorkManager;

		public CqrsInterceptor(EventBag eventBag, IBoltOnLogger<CqrsInterceptor> logger,
			IEventDispatcher eventDispatcher, 
			ICqrsRepositoryFactory cqrsRepositoryFactory,
			IUnitOfWorkManager unitOfWorkManager)
		{
			_eventBag = eventBag;
			_logger = logger;
			_eventDispatcher = eventDispatcher;
			_cqrsRepositoryFactory = cqrsRepositoryFactory;
			_unitOfWorkManager = unitOfWorkManager;
		}

		public TResponse Run<TRequest, TResponse>(IRequest<TResponse> request,
			Func<IRequest<TResponse>, TResponse> next) where TRequest : IRequest<TResponse>
		{
			var response = next(request);
			foreach (var @event in _eventBag.Events)
			{
				_logger.Debug($"Publishing event: {@event.Id} {@event.SourceTypeName}");
			}

			return response;
		}

		public async Task<TResponse> RunAsync<TRequest, TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken,
			Func<IRequest<TResponse>, CancellationToken, Task<TResponse>> next) where TRequest : IRequest<TResponse>
		{
			var response = await next(request, cancellationToken);
			// creating a new list by calling .ToList() as the events in the original list need to be removed
			foreach (var @event in _eventBag.Events.ToList())
			{
				_logger.Debug($"Publishing event. Id: {@event.Id} SourceType: {@event.SourceTypeName}");
				await _eventDispatcher.DispatchAsync(@event, cancellationToken);
				_eventBag.Events.Remove(@event);
			}

			if (request is CqrsEvent cqrsEvent)
			{
				_logger.Debug($"Building repository. SourceType: {cqrsEvent.SourceTypeName}");
                var method = _cqrsRepositoryFactory.GetType().GetMethod("GetRepository");
				var sourceEntityType = Type.GetType(cqrsEvent.SourceTypeName);
				var generic = method.MakeGenericMethod(sourceEntityType);
				dynamic repository = generic.Invoke(_cqrsRepositoryFactory, null);
				_logger.Debug("Built repository");
				using (var uow = _unitOfWorkManager.Get(u => u.TransactionScopeOption = TransactionScopeOption.RequiresNew))
				{
					var cqrsEntity = await repository.GetByIdAsync(cqrsEvent.SourceId);
					var baseCqrsEntity = cqrsEntity as BaseCqrsEntity;
					_logger.Debug($"Fetched BaseCqrsEntity. Id: {cqrsEvent.SourceId}");
					var @event = baseCqrsEntity?.Events.FirstOrDefault(f => f.Id == cqrsEvent.Id);
					if (@event != null)
					{
						_logger.Debug("Removing event...");
						baseCqrsEntity.Events.Remove(@event);
						await repository.UpdateAsync(cqrsEntity);
						_logger.Debug($"Removed event. Id: {@event.Id}");
					}
					uow.Commit();
				}
			}

			return response;
		}

		public void Dispose()
		{
		}
	}
}
