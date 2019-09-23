using System;
using System.Linq;
using System.Threading.Tasks;
using BoltOn.Data;
using BoltOn.Logging;
using BoltOn.UoW;
using System.Transactions;

namespace BoltOn.Cqrs
{
	public interface IProcessedEventPurger
	{
		Task PurgeAsync(ICqrsEvent cqrsEvent);
	}

	public class ProcessedEventPurger : IProcessedEventPurger
    {
        private readonly IBoltOnLogger<ProcessedEventPurger> _logger;
        private readonly ICqrsRepositoryFactory _cqrsRepositoryFactory;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public ProcessedEventPurger(IBoltOnLogger<ProcessedEventPurger> logger,
            ICqrsRepositoryFactory cqrsRepositoryFactory,
            IUnitOfWorkManager unitOfWorkManager)
        {
            _logger = logger;
            _cqrsRepositoryFactory = cqrsRepositoryFactory;
            _unitOfWorkManager = unitOfWorkManager;
        }

        public async Task PurgeAsync(ICqrsEvent cqrsEvent)
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
    }
}
