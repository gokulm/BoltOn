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
			var method = _cqrsRepositoryFactory.GetType().GetMethod("GetRepository");
			var sourceEntityType = Type.GetType(request.SourceTypeName);
			var generic = method.MakeGenericMethod(sourceEntityType);
			dynamic repository = generic.Invoke(_cqrsRepositoryFactory, null);
			_logger.Debug("Built repository");
			using (var uow = _unitOfWorkManager.Get(u => u.TransactionScopeOption = TransactionScopeOption.RequiresNew))
			{
				var cqrsEntity = await repository.GetByIdAsync(request.SourceId, cancellationToken);
				var baseCqrsEntity = cqrsEntity as BaseCqrsEntity;
				_logger.Debug($"Fetched BaseCqrsEntity. Id: {request.SourceId}");
				var @event = baseCqrsEntity?.EventsToBeProcessed.FirstOrDefault(f => f.Id == request.Id);
				if (@event != null)
				{
					_logger.Debug("Removing event...");
					baseCqrsEntity.RemoveEventsToBeProcessed(@event);
					await repository.UpdateAsync(cqrsEntity, cancellationToken);
					_logger.Debug($"Removed event. Id: {@event.Id}");
				}
				uow.Commit();
			}
		}
	}
}
