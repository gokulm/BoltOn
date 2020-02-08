using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
			var getRepositoryMethod = _cqrsRepositoryFactory.GetType().GetMethod("GetRepository");

			await RemoveEventToBeProcessed(request, getRepositoryMethod, cancellationToken);
			await RemoveProcessedEvent(request, getRepositoryMethod, cancellationToken);
		}

		private async Task RemoveEventToBeProcessed(CqrsEventProcessedEvent request, System.Reflection.MethodInfo getRepositoryMethod, CancellationToken cancellationToken)
		{
			_logger.Debug($"Getting source entity repository. SourceTypeName: {request.SourceTypeName}");
			var sourceEntityType = Type.GetType(request.SourceTypeName);
			var genericMethodForSourceEntity = getRepositoryMethod.MakeGenericMethod(sourceEntityType);
			dynamic sourceEntityRepository = genericMethodForSourceEntity.Invoke(_cqrsRepositoryFactory, null);
			_logger.Debug($"Built {nameof(CqrsRepositoryFactory)}");
			using (var uow = _unitOfWorkManager.Get())
			{
				_logger.Debug($"Fetched source entity by Id. Id: {request.SourceId}");
				var cqrsEntity = await sourceEntityRepository.GetByIdAsync(request.SourceId, cancellationToken);
				var baseCqrsEntity = cqrsEntity as BaseCqrsEntity;
				_logger.Debug($"Fetched BaseCqrsEntity. Id: {request.SourceId}");
				var @event = baseCqrsEntity?.EventsToBeProcessed.FirstOrDefault(f => f.Id == request.Id);
				if (@event != null)
				{
					_logger.Debug($"Removing event. Id: {@event.Id}");
					baseCqrsEntity.RemoveEventToBeProcessed(@event);
					await sourceEntityRepository.UpdateAsync(cqrsEntity, cancellationToken);
					_logger.Debug($"Removed event");
				}
				uow.Commit();
			}
		}

		private async Task RemoveProcessedEvent(CqrsEventProcessedEvent request, System.Reflection.MethodInfo getRepositoryMethod, CancellationToken cancellationToken)
		{
			_logger.Debug($"Getting destination entity repository. DestinationTypeName: {request.DestinationTypeName}");
			var destinationEntityType = Type.GetType(request.DestinationTypeName);
			var genericMethodForDestinationEntity = getRepositoryMethod.MakeGenericMethod(destinationEntityType);
			dynamic destinationEntityRepository = genericMethodForDestinationEntity.Invoke(_cqrsRepositoryFactory, null);
			_logger.Debug($"Built {nameof(CqrsRepositoryFactory)}");
			using (var uow = _unitOfWorkManager.Get())
			{
				_logger.Debug($"Fetched destination entity by Id. Id: {request.DestinationId}");
				var cqrsEntity = await destinationEntityRepository.GetByIdAsync(request.DestinationId, cancellationToken);
				var baseCqrsEntity = cqrsEntity as BaseCqrsEntity;
				_logger.Debug($"Fetched BaseCqrsEntity. Id: {request.DestinationId}");
				var @event = baseCqrsEntity?.ProcessedEvents.FirstOrDefault(f => f.Id == request.Id);
				if (@event != null)
				{
					_logger.Debug($"Removing event. Id: {@event.Id}");
					baseCqrsEntity.RemoveProcessedEvent(@event);
					await destinationEntityRepository.UpdateAsync(cqrsEntity, cancellationToken);
					_logger.Debug($"Removed event");
				}
				uow.Commit();
			}
		}
	}
}
