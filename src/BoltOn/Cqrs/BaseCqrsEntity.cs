using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BoltOn.Data;
using BoltOn.Logging;
using BoltOn.Mediator.Interceptors;
using BoltOn.Mediator.Pipeline;

namespace BoltOn.Cqrs
{
	public interface ICqrsEntity
	{
		List<IEvent> Events { get; set; }
		bool IsDisbursed { get; set; }
	}

	public abstract class BaseCqrsEntity<TIdType> : BaseEntity<TIdType>, ICqrsEntity
    {
        public List<IEvent> Events { get; set; }

        public bool IsDisbursed { get; set; }

        protected void RaiseEvent(IEvent @event)
        {
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

		public EventDispatcher(IBoltOnLogger<EventDispatcher> logger)
		{
			_logger = logger;
		}

		public async Task PublishAsync(IEvent @event, CancellationToken cancellationToken = default)
		{
			_logger.Debug($"Publishing event: {@event.Id} {@event.SourceTypeName}");
			await Task.FromResult(1);
		}
	}

	public class CqrsInterceptor : IInterceptor
	{
		private readonly IEventHub _eventHub;
		private readonly IBoltOnLogger<CqrsInterceptor> _logger;
		private readonly IEventDispatcher _eventDispatcher;

		public CqrsInterceptor(IEventHub eventHub, IBoltOnLogger<CqrsInterceptor> logger,
			IEventDispatcher eventDispatcher)
		{
			_eventHub = eventHub;
			_logger = logger;
			_eventDispatcher = eventDispatcher;
		}

		public TResponse Run<TRequest, TResponse>(IRequest<TResponse> request, 
			Func<IRequest<TResponse>, TResponse> next) where TRequest : IRequest<TResponse>
		{
			var response = next(request);
			foreach (var @event in _eventHub.Events)
			{
				_logger.Debug($"Publishing event: {@event.Id} {@event.SourceTypeName}");
			}

			return response;
		}

		public async Task<TResponse> RunAsync<TRequest, TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken, 
			Func<IRequest<TResponse>, CancellationToken, Task<TResponse>> next) where TRequest : IRequest<TResponse>
		{
			var response = await next(request, cancellationToken);
			foreach (var @event in _eventHub.Events)
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
