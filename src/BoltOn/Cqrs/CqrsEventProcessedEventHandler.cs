using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using BoltOn.Logging;
using BoltOn.Mediator.Pipeline;
using BoltOn.UoW;

namespace BoltOn.Cqrs
{
	public class CqrsEventProcessedEvent : CqrsEvent
	{
	}

	public class CqrsEventProcessedEventHandler : IHandler<CqrsEventProcessedEvent>
    {
		private readonly ICqrsRepositoryFactory _cqrsRepositoryFactory;
		private readonly IBoltOnLogger<CqrsEventProcessedEventHandler> _logger;
		private readonly IUnitOfWorkManager _unitOfWorkManager;

		public CqrsEventProcessedEventHandler(ICqrsRepositoryFactory cqrsRepositoryFactory,
			IBoltOnLogger<CqrsEventProcessedEventHandler> logger,
			IUnitOfWorkManager unitOfWorkManager)
		{
			_cqrsRepositoryFactory = cqrsRepositoryFactory;
			_logger = logger;
			_unitOfWorkManager = unitOfWorkManager;
		}

		public async Task HandleAsync(CqrsEventProcessedEvent request, CancellationToken cancellationToken)
		{
			_logger.Debug($"{nameof(CqrsEventProcessedEventHandler)} invoked");
			await RemoveEventToBeProcessed(request, cancellationToken);
		}

		private async Task RemoveEventToBeProcessed(CqrsEventProcessedEvent request, CancellationToken cancellationToken)
		{
			var getRepositoryMethod = _cqrsRepositoryFactory.GetType().GetMethod("GetRepository");
			_logger.Debug($"Getting source entity repository. SourceTypeName: {request.SourceTypeName}");
			var sourceEntityType = Type.GetType(request.SourceTypeName);
			var genericMethodForSourceEntity = getRepositoryMethod.MakeGenericMethod(sourceEntityType);
			dynamic repository = genericMethodForSourceEntity.Invoke(_cqrsRepositoryFactory, null);
			_logger.Debug($"Built {nameof(CqrsRepositoryFactory)}");
			using (var uow = _unitOfWorkManager.Get(u => u.TransactionScopeOption = TransactionScopeOption.RequiresNew))
			{
				_logger.Debug($"Fetched source entity by Id. Id: {request.SourceId}");
				var cqrsEntity = await repository.GetByIdAsync(request.SourceId, cancellationToken);
				var baseCqrsEntity = cqrsEntity as BaseCqrsEntity;
				_logger.Debug($"Fetched BaseCqrsEntity. Id: {request.SourceId}");
				var @event = baseCqrsEntity?.EventsToBeProcessed.FirstOrDefault(f => f.Id == request.Id);
				if (@event != null)
				{
					_logger.Debug($"Removing event. Id: {@event.Id}");
					baseCqrsEntity.RemoveEventsToBeProcessed(@event);
					await repository.UpdateAsync(cqrsEntity, cancellationToken);
					_logger.Debug($"Removed event");
				}
				uow.Commit();
			}
		}
	}
}
