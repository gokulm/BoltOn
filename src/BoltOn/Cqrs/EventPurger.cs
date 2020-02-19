using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BoltOn.Logging;
using BoltOn.UoW;

namespace BoltOn.Cqrs
{
	public interface IEventPurger
	{
		Task PurgeAsync(ICqrsEvent cqrsEvent, CancellationToken cancellationToken);
	}

	public class EventPurger : IEventPurger
	{
		private readonly IBoltOnLogger<IEventPurger> _logger;
		private readonly ICqrsRepositoryFactory _cqrsRepositoryFactory;
		private readonly IUnitOfWorkManager _unitOfWorkManager;

		public EventPurger(IBoltOnLogger<IEventPurger> logger,
			ICqrsRepositoryFactory cqrsRepositoryFactory,
			IUnitOfWorkManager unitOfWorkManager)
		{
			_logger = logger;
			_cqrsRepositoryFactory = cqrsRepositoryFactory;
			_unitOfWorkManager = unitOfWorkManager;
		}

		public async Task PurgeAsync(ICqrsEvent cqrsEvent, CancellationToken cancellationToken)
		{
			_logger.Debug($"Getting entity repository. TypeName: {cqrsEvent.SourceTypeName}");
			var sourceEntityType = Type.GetType(cqrsEvent.SourceTypeName);
			var getRepositoryMethod = _cqrsRepositoryFactory.GetType().GetMethod("GetRepository");
            if (getRepositoryMethod != null)
            {
                var genericMethodForSourceEntity = getRepositoryMethod.MakeGenericMethod(sourceEntityType);
                dynamic sourceEntityRepository = genericMethodForSourceEntity.Invoke(_cqrsRepositoryFactory, null);
                _logger.Debug($"Built {nameof(CqrsRepositoryFactory)}");
                _logger.Debug($"Fetching entity by Id. Id: {cqrsEvent.SourceId}");
                using var uow = _unitOfWorkManager.Get();
                var cqrsEntity = await sourceEntityRepository.GetByIdAsync(cqrsEvent.SourceId, cancellationToken);
                var baseCqrsEntity = cqrsEntity as BaseCqrsEntity;
                _logger.Debug($"Fetched entity. Id: {cqrsEvent.SourceId}");
                var @event = baseCqrsEntity?.EventsToBeProcessed.FirstOrDefault(f => f.Id == cqrsEvent.Id);
                if (@event != null)
                {
                    _logger.Debug($"Removing event. Id: {@event.Id}");
                    baseCqrsEntity.RemoveEventToBeProcessed(@event);
                    await sourceEntityRepository.UpdateAsync(cqrsEntity, cancellationToken);
                    _logger.Debug("Removed event");
                }
                uow.Commit();
            }
        }
	}
}
