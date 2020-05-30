using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BoltOn.Bootstrapping;
using BoltOn.Cqrs;
using Microsoft.EntityFrameworkCore;

namespace BoltOn.Data.EF
{
	public class CqrsRepository<TEntity, TDbContext> : Repository<TEntity, TDbContext>
		where TDbContext : DbContext
		where TEntity : BaseCqrsEntity
	{
		private readonly EventBag _eventBag;
		private readonly CqrsOptions _cqrsOptions;

		public CqrsRepository(IDbContextFactory dbContextFactory, EventBag eventBag,
			CqrsOptions cqrsOptions) : base(dbContextFactory)
		{
			_eventBag = eventBag;
			_cqrsOptions = cqrsOptions;
		}

		protected override async Task SaveChangesAsync(TEntity entity, CancellationToken cancellationToken = default)
		{
			PublishEvents(entity);
			await DbContext.SaveChangesAsync(cancellationToken);
		}

		protected override async Task SaveChangesAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
		{
			foreach (var entity in entities)
			{
				PublishEvents(entity);
			}
			await DbContext.SaveChangesAsync(cancellationToken);
		}

		private void PublishEvents(TEntity entity)
		{
			if (entity is BaseCqrsEntity baseCqrsEntity)
			{
				baseCqrsEntity.EventsToBeProcessed.ToList().ForEach(e => _eventBag.EventsToBeProcessed.Add(e));

				if(baseCqrsEntity.ProcessedEvents.Any() && _cqrsOptions.PurgeEventsProcessedBefore.HasValue)
				{
					var timeStamp = DateTime.UtcNow.Add(-1 * _cqrsOptions.PurgeEventsProcessedBefore.Value);
					var processedEventsToBeRemoved = baseCqrsEntity.ProcessedEvents.Where(w => w.ProcessedDate < timeStamp).ToList();
					processedEventsToBeRemoved.ForEach(e => baseCqrsEntity.RemoveProcessedEvent(e));
				}
			}
		}
	}
}