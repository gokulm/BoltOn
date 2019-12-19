using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BoltOn.Logging;

namespace BoltOn.Cqrs
{
	public interface IEventPurger
	{
		Task PurgeAsync(ICqrsEvent cqrsEvent, CancellationToken cancellationToken);
	}

    public class EventPurger : IEventPurger
    {
        private readonly IBoltOnLogger<EventPurger> _logger;
        private readonly ICqrsRepositoryFactory _cqrsRepositoryFactory;

        public EventPurger(IBoltOnLogger<EventPurger> logger,
            ICqrsRepositoryFactory cqrsRepositoryFactory)
        {
            _logger = logger;
            _cqrsRepositoryFactory = cqrsRepositoryFactory;
        }

		public async Task PurgeAsync(ICqrsEvent cqrsEvent, CancellationToken cancellationToken)
        {
            _logger.Debug($"Getting entity repository. TypeName: {cqrsEvent.SourceTypeName}");
            var sourceEntityType = Type.GetType(cqrsEvent.SourceTypeName);
            var getRepositoryMethod = _cqrsRepositoryFactory.GetType().GetMethod("GetRepository");
            var genericMethodForSourceEntity = getRepositoryMethod.MakeGenericMethod(sourceEntityType);
            dynamic sourceEntityRepository = genericMethodForSourceEntity.Invoke(_cqrsRepositoryFactory, null);
            _logger.Debug($"Built {nameof(CqrsRepositoryFactory)}");
            _logger.Debug($"Fetched entity by Id. Id: {cqrsEvent.SourceId}");
            var cqrsEntity = await sourceEntityRepository.GetByIdAsync(cqrsEvent.SourceId, cancellationToken);
            var baseCqrsEntity = cqrsEntity as BaseCqrsEntity;
            _logger.Debug($"Fetched BaseCqrsEntity. Id: {cqrsEvent.SourceId}");
            var @event = baseCqrsEntity?.EventsToBeProcessed.FirstOrDefault(f => f.Id == cqrsEvent.Id);
            if (@event != null)
            {
                _logger.Debug($"Removing event. Id: {@event.Id}");
                baseCqrsEntity.RemoveEventToBeProcessed(@event);
                await sourceEntityRepository.UpdateAsync(cqrsEntity, cancellationToken);
                _logger.Debug($"Removed event");
            }
        }
    }
}
