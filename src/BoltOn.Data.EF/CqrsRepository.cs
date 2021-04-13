using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using BoltOn.Bus;
using BoltOn.Cqrs;
using Microsoft.EntityFrameworkCore;

namespace BoltOn.Data.EF
{
	public class CqrsRepository<TEntity, TDbContext> : Repository<TEntity, TDbContext>
		where TDbContext : DbContext
		where TEntity : BaseDomainEntity
	{
		private readonly IAppServiceBus _bus;
		private readonly IRepository<EventStore> _eventStoreRepository;

		public CqrsRepository(TDbContext dbContext,
			IAppServiceBus bus,
			IRepository<EventStore> repository) : base(dbContext)
		{
			_bus = bus;
			_eventStoreRepository = repository;
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
				foreach (var entity in entitiesList)
				{
					await AddEvents(entity, cancellationToken);
				}

				await DbContext.SaveChangesAsync(cancellationToken);
				transactionScope.Complete();
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
					EventId = @event.Id,
					EntityId = entity.DomainEntityId,
					EntityType = entity.GetType().FullName,
					CreatedDate = System.DateTimeOffset.Now,
					Data = @event
				};

				await _eventStoreRepository.AddAsync(eventStore, cancellationToken);
			}
		}

		protected async virtual Task PublishEventsAndPurge(TEntity entity, CancellationToken cancellationToken)
		{
			var eventStoreList = (await _eventStoreRepository.FindByAsync(f => f.EntityId == entity.DomainEntityId &&
					f.EntityType == entity.GetType().FullName)).ToList();

			foreach (var eventStore in eventStoreList)
			{
				using var transactionScope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions
				{
					IsolationLevel = IsolationLevel.ReadCommitted
				}, TransactionScopeAsyncFlowOption.Enabled);
				await _eventStoreRepository.DeleteAsync(eventStore.EventId, cancellationToken);
				await _bus.PublishAsync(eventStore.Data, cancellationToken);
				transactionScope.Complete();
			}
		}

		protected async virtual Task PublishEvents(TEntity entity, CancellationToken cancellationToken)
		{
			var eventStoreList = (await _eventStoreRepository.FindByAsync(f => f.EntityId == entity.DomainEntityId &&
					f.EntityType == entity.GetType().FullName && !f.ProcessedDate.HasValue)).ToList();

			foreach (var eventStore in eventStoreList)
			{
				using var transactionScope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions
				{
					IsolationLevel = IsolationLevel.ReadCommitted
				}, TransactionScopeAsyncFlowOption.Enabled);
				eventStore.ProcessedDate = DateTimeOffset.Now;
				await _eventStoreRepository.UpdateAsync(eventStore, cancellationToken);
				await _bus.PublishAsync(eventStore.Data, cancellationToken);
				transactionScope.Complete();
			}
		}
	}
}