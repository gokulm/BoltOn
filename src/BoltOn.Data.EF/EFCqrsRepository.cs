using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BoltOn.Cqrs;
using BoltOn.Utilities;
using Microsoft.EntityFrameworkCore;

namespace BoltOn.Data.EF
{
	public class EFCqrsRepository<TEntity, TDbContext> : EFRepository<TEntity, TDbContext>, IRepository<TEntity>
		where TEntity : BaseCqrsEntity
		where TDbContext : DbContext
	{
		private readonly EventBag _eventBag;
		private readonly IBoltOnClock _boltOnClock;

		public EFCqrsRepository(IDbContextFactory dbContextFactory, EventBag eventBag,
			IBoltOnClock boltOnClock) : base(dbContextFactory)
		{
			_eventBag = eventBag;
			_boltOnClock = boltOnClock;
		}

		protected override void SaveChanges(TEntity entity)
		{
			PublishEvents(entity);
			base.SaveChanges(entity);
		}

		protected override async Task SaveChangesAsync(TEntity entity, CancellationToken cancellationToken = default)
		{
			PublishEvents(entity);
			await base.SaveChangesAsync(entity, cancellationToken);
		}

		private void PublishEvents(TEntity entity)
		{
			var eventsToBeProcessed = entity.EventsToBeProcessed.ToList()
				.Where(w => !w.CreatedDate.HasValue);
			foreach (var @event in eventsToBeProcessed)
			{
				@event.CreatedDate = _boltOnClock.Now;
				_eventBag.EventsToBeProcessed.Add(@event);
			}

			var processedEvents = entity.ProcessedEvents.ToList()
				.Where(w => !w.ProcessedDate.HasValue);
			foreach (var @event in processedEvents)
			{
				@event.ProcessedDate = _boltOnClock.Now;
			}
		}
	}
}
