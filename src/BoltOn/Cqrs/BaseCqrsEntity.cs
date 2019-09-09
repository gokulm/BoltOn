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

	public class CqrsInterceptor : IInterceptor
	{
		private readonly IEventHub _eventHub;
		private readonly IBoltOnLogger<CqrsInterceptor> _logger;

		public CqrsInterceptor(IEventHub eventHub, IBoltOnLogger<CqrsInterceptor> logger)
		{
			_eventHub = eventHub;
			_logger = logger;
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
			return response;
		}

		public void Dispose()
		{
		}
	}
}
