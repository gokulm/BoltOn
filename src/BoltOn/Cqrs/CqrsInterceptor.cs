using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BoltOn.Data;
using BoltOn.Logging;
using BoltOn.Mediator.Interceptors;
using BoltOn.Mediator.Pipeline;

namespace BoltOn.Cqrs
{
	public class CqrsInterceptor : IInterceptor
	{
		private readonly EventBag _eventBag;
		private readonly IBoltOnLogger<CqrsInterceptor> _logger;
		private readonly IEventDispatcher _eventDispatcher;
		private readonly ICqrsRepositoryFactory _cqrsRepositoryFactory;

		public CqrsInterceptor(EventBag eventBag, IBoltOnLogger<CqrsInterceptor> logger,
			IEventDispatcher eventDispatcher, 
			ICqrsRepositoryFactory cqrsRepositoryFactory)
		{
			_eventBag = eventBag;
			_logger = logger;
			_eventDispatcher = eventDispatcher;
			_cqrsRepositoryFactory = cqrsRepositoryFactory;
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
			// created a new list as the events in the original list need to be removed
			var events = _eventBag.Events.ToList();
			foreach (var @event in events)
			{
				_logger.Debug($"Publishing event: {@event.Id} {@event.SourceTypeName}");
				await _eventDispatcher.PublishAsync(@event);
				_eventBag.Events.Remove(@event);
			}

			if (request is BoltOnEvent)
			{
				_logger.Debug("Removing event from entity...");
				var boltOnEvent = request as BoltOnEvent;
				var method = typeof(CqrsRepositoryFactory).GetMethod("GetRepository");
				var sourceEntityType = Type.GetType((boltOnEvent).SourceTypeName);
				var generic = method.MakeGenericMethod(sourceEntityType);
				dynamic repository = generic.Invoke(_cqrsRepositoryFactory, null);
				_logger.Debug("Built repository...");
				var cqrsEntity = await repository.GetByIdAsync(boltOnEvent.SourceId);
				var baseCqrsEntity = cqrsEntity as BaseCqrsEntity;
				var @event = baseCqrsEntity.Events.FirstOrDefault(f => f.Id == boltOnEvent.Id);
				if (@event != null)
				{
					baseCqrsEntity.Events.Remove(@event);
					await repository.UpdateAsync(cqrsEntity);
				}
			}

			return response;
		}

		public void Dispose()
		{
		}
	}
}
