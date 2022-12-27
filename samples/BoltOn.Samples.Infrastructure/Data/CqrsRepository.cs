using System;
using BoltOn.Bus;
using BoltOn.Logging;
using BoltOn.Samples.Application.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using BoltOn.Data.EF;
using System.Linq;

namespace BoltOn.Samples.Infrastructure.Data
{
	public class CqrsRepository<TEntity, TDbContext> : Repository<TEntity, TDbContext>
		where TDbContext : DbContext
		where TEntity : BaseDomainEntity
	{
		private readonly IAppServiceBus _bus;
		private readonly IRepository<EventStore> _eventStoreRepository;
		private readonly IAppLogger<CqrsRepository<TEntity, TDbContext>> _appLogger;

		public CqrsRepository(TDbContext dbContext,
			IAppServiceBus bus,
			IRepository<EventStore> repository,
			IAppLoggerFactory appLoggerFactory) : base(dbContext)
		{
			_bus = bus;
			_eventStoreRepository = repository;
			_appLogger = appLoggerFactory.Create<CqrsRepository<TEntity, TDbContext>>();
		}

		protected override async Task SaveChangesAsync(TEntity entity, CancellationToken cancellationToken = default)
		{
			await SaveChangesAsync(new[] { entity }, cancellationToken);
		}

		protected override async Task SaveChangesAsync(IEnumerable<TEntity> entities,
			CancellationToken cancellationToken = default)
		{
			var entitiesList = entities.ToList();
			using (var transactionScope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions
			{
				IsolationLevel = IsolationLevel.ReadCommitted
			}, TransactionScopeAsyncFlowOption.Enabled))
			{
				_appLogger.Debug($"Adding entities. Count: {entitiesList.Count}");
				foreach (var entity in entitiesList)
				{
					await AddEvents(entity, cancellationToken);
				}

				await DbContext.SaveChangesAsync(cancellationToken);
				transactionScope.Complete();
				_appLogger.Debug("Transaction complete. Added entities.");
			}

			foreach (var entity in entitiesList)
			{
				if (entity.PurgeEvents)
					await PublishEventsAndPurge(entity, cancellationToken);
				else
					await PublishEvents(entity, cancellationToken);
			}
		}

		protected async virtual Task AddEvents(TEntity entity, CancellationToken cancellationToken)
		{
			foreach (var @event in entity.EventsToBeProcessed.ToList())
			{
				var eventStore = new EventStore
				{
					EventId = @event.EventId,
					EntityId = entity.DomainEntityId,
					EntityType = entity.GetType().FullName,
					CreatedDate = System.DateTimeOffset.Now,
					Data = @event
				};

				await _eventStoreRepository.AddAsync(eventStore, cancellationToken);
				_appLogger.Debug($"Added event. EventId: {eventStore.EventId}");
			}
		}

		protected async virtual Task PublishEventsAndPurge(TEntity entity, CancellationToken cancellationToken)
		{
			var eventStoreList = (await _eventStoreRepository.FindByAsync(f => f.EntityId == entity.DomainEntityId &&
					f.EntityType == entity.GetType().FullName, cancellationToken)).OrderBy(o => o.CreatedDate).ToList();

			_appLogger.Debug($"Publishing events and purging. No. of events to be purged: {eventStoreList.Count}");

			foreach (var eventStore in eventStoreList)
			{
				using var transactionScope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions
				{
					IsolationLevel = IsolationLevel.ReadCommitted
				}, TransactionScopeAsyncFlowOption.Enabled);
				await _eventStoreRepository.DeleteAsync(eventStore.EventId, cancellationToken);
				entity.RemoveEventToBeProcessed(eventStore.Data);
				_appLogger.Debug($"Removed event. EventId: {eventStore.EventId}");
				await _bus.PublishAsync(eventStore.Data, cancellationToken);
				_appLogger.Debug($"Published event. EventId: {eventStore.EventId}");
				transactionScope.Complete();
				_appLogger.Debug($"Transaction complete. Removed and published successfully. EventId: {eventStore.EventId}");
			}
		}

		protected async virtual Task PublishEvents(TEntity entity, CancellationToken cancellationToken)
		{
			var eventStoreList = (await _eventStoreRepository.FindByAsync(f => f.EntityId == entity.DomainEntityId &&
					f.EntityType == entity.GetType().FullName && !f.ProcessedDate.HasValue)).OrderBy(o => o.CreatedDate).ToList();
			_appLogger.Debug($"Publishing events and updating. No. of events to be updated: {eventStoreList.Count}");

			foreach (var eventStore in eventStoreList)
			{
				using var transactionScope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions
				{
					IsolationLevel = IsolationLevel.ReadCommitted
				}, TransactionScopeAsyncFlowOption.Enabled);
				eventStore.ProcessedDate = DateTimeOffset.Now;
				await _eventStoreRepository.UpdateAsync(eventStore, cancellationToken);
				_appLogger.Debug($"Updated event. EventId: {eventStore.EventId}");
				await _bus.PublishAsync(eventStore.Data, cancellationToken);
				_appLogger.Debug($"Published event. EventId: {eventStore.EventId}");
				transactionScope.Complete();
				_appLogger.Debug($"Transaction complete. Updated and published successfully. EventId: {eventStore.EventId}");
			}
		}
	}
}

