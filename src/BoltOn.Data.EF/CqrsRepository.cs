using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using BoltOn.Bus;
using BoltOn.Cqrs;
using BoltOn.Logging;
using Microsoft.EntityFrameworkCore;

namespace BoltOn.Data.EF
{
	public class CqrsRepository<TEntity, TDbContext> : Repository<TEntity, TDbContext>
		where TDbContext : DbContext
		where TEntity : BaseDomainEntity
	{
		private readonly IAppServiceBus _bus;
		private readonly IAppLogger<CqrsRepository<TEntity, TDbContext>> _appLogger;

		public CqrsRepository(TDbContext dbContext,
			IAppServiceBus bus,
			IRepository<EventStore> repository,
			IAppLoggerFactory appLoggerFactory) : base(dbContext)
		{
			_bus = bus;
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

				await DbContext.Set<EventStore>().AddAsync(eventStore, cancellationToken);
			}
		}

		protected async virtual Task PublishEventsAndPurge(TEntity entity, CancellationToken cancellationToken)
		{
			var eventStoreList = await DbContext.Set<EventStore>().Where(f => f.EntityId == entity.DomainEntityId &&
					f.EntityType == entity.GetType().FullName && !f.ProcessedDate.HasValue).OrderBy(o => o.CreatedDate).ToListAsync();

			foreach (var eventStore in eventStoreList)
			{
				using var transactionScope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions
				{
					IsolationLevel = IsolationLevel.ReadCommitted
				}, TransactionScopeAsyncFlowOption.Enabled);
				DbContext.Set<EventStore>().Remove(eventStore);
				entity.RemoveEventToBeProcessed(eventStore.Data);
				await _bus.PublishAsync(eventStore.Data, cancellationToken);
				transactionScope.Complete();
			}
		}

		protected async virtual Task PublishEvents(TEntity entity, CancellationToken cancellationToken)
		{
			var eventStoreList = await DbContext.Set<EventStore>().Where(f => f.EntityId == entity.DomainEntityId &&
					f.EntityType == entity.GetType().FullName && !f.ProcessedDate.HasValue).OrderBy(o => o.CreatedDate).ToListAsync();

			foreach (var eventStore in eventStoreList)
			{
				using var transactionScope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions
				{
					IsolationLevel = IsolationLevel.ReadCommitted
				}, TransactionScopeAsyncFlowOption.Enabled);
				eventStore.ProcessedDate = DateTimeOffset.Now;
				DbContext.Set<EventStore>().Update(eventStore);
				await _bus.PublishAsync(eventStore.Data, cancellationToken);
				transactionScope.Complete();
			}
		}
	}
}