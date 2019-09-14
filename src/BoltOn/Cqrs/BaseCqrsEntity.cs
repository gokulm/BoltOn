using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BoltOn.Bus;
using BoltOn.Data;
using BoltOn.Logging;
using BoltOn.Mediator.Interceptors;
using BoltOn.Mediator.Pipeline;

namespace BoltOn.Cqrs
{
	public interface ICqrsEntity
	{
		List<BoltOnEvent> Events { get; set; }
		bool IsDisbursed { get; set; }
	}

	public abstract class BaseCqrsEntity<TIdType> : BaseEntity<TIdType>, ICqrsEntity
    {
		public List<BoltOnEvent> Events { get; set; } = new List<BoltOnEvent>();

        public bool IsDisbursed { get; set; }

        protected void RaiseEvent(BoltOnEvent @event)
        {
			IsDisbursed = true;

			if (@event.Id != Guid.Empty)
				@event.Id = Guid.NewGuid();

            Events.Add(@event);
        }
    }

	public interface IEventDispatcher
	{
		Task PublishAsync(IEvent @event, CancellationToken cancellationToken = default);
	}

	public class EventDispatcher : IEventDispatcher
	{
		private readonly IBoltOnLogger<EventDispatcher> _logger;
		private readonly IBus _bus;

		public EventDispatcher(IBoltOnLogger<EventDispatcher> logger,
			IBus bus)
		{
			_logger = logger;
			_bus = bus;
		}

		public async Task PublishAsync(IEvent @event, CancellationToken cancellationToken = default)
		{
			_logger.Debug($"Publishing event: {@event.Id} {@event.SourceTypeName}");
			await _bus.PublishAsync(@event);
			await Task.FromResult(1);
		}
	}

	public class CqrsInterceptor : IInterceptor
	{
		private readonly EventBag _eventBag;
		private readonly IBoltOnLogger<CqrsInterceptor> _logger;
		private readonly IEventDispatcher _eventDispatcher;

		public CqrsInterceptor(EventBag eventBag, IBoltOnLogger<CqrsInterceptor> logger,
			IEventDispatcher eventDispatcher)
		{
			_eventBag = eventBag;
			_logger = logger;
			_eventDispatcher = eventDispatcher;
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
			foreach (var @event in _eventBag.Events)
			{
				_logger.Debug($"Publishing event: {@event.Id} {@event.SourceTypeName}");
				await _eventDispatcher.PublishAsync(@event);
			}
			return response;
		}

		public void Dispose()
		{
		}
	}
}
