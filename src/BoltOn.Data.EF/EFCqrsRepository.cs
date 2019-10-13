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
			if (entity is ICqrsEntity)
			{
				var cqrsEntity = entity as ICqrsEntity;
				foreach (var @event in cqrsEntity.EventsToBeProcessed)
				{
					SetCreatedDate(@event);
					_eventBag.Events.Add(@event);
				}

				foreach (var @event in cqrsEntity.ProcessedEvents)
					SetCreatedDate(@event);
			}
		}

		private void SetCreatedDate(ICqrsEvent @event)
		{
			if (!@event.CreatedDate.HasValue)
				@event.CreatedDate = _boltOnClock.Now;
		}
	}
}
