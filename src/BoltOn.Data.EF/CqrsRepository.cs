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

		public CqrsRepository(TDbContext dbContext, EventBag eventBag,
			CqrsOptions cqrsOptions) : base(dbContext)
		{
			_eventBag = eventBag;
			_cqrsOptions = cqrsOptions;
		}

		protected override async Task SaveChangesAsync(TEntity entity, CancellationToken cancellationToken = default)
		{
			await SaveChangesAsync(new[] { entity }, cancellationToken);
		}

		protected override async Task SaveChangesAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
		{
			foreach (var entity in entities)
			{
				PublishEvents(entity);
			}
			await DbContext.SaveChangesAsync(cancellationToken);
		}

		protected virtual void PublishEvents(TEntity entity)
		{
			entity.EventsToBeProcessed.ToList().ForEach(e =>
			{
				_eventBag.AddEventToBeProcessed(e, async (e) => await RemoveEventToBeProcessed(e));
			});

			if (entity.ProcessedEvents.Any() && _cqrsOptions.PurgeEventsProcessedBefore.HasValue)
			{
				var timeStamp = DateTime.UtcNow.Add(-1 * _cqrsOptions.PurgeEventsProcessedBefore.Value);
				var processedEventsToBeRemoved = entity.ProcessedEvents.Where(w => w.ProcessedDate < timeStamp).ToList();
				processedEventsToBeRemoved.ForEach(e => entity.RemoveProcessedEvent(e));
			}
		}

		private async Task RemoveEventToBeProcessed(ICqrsEvent @event)
		{
			var entity = await GetByIdAsync(@event.SourceId);
			entity.RemoveEventToBeProcessed(@event);
			await UpdateAsync(entity);
			await DbContext.SaveChangesAsync();
		}
	}
}