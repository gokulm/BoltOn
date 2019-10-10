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

		public override async Task<TEntity> GetByIdAsync(object id, CancellationToken cancellationToken = default)
		{
			var stringId = id.ToString();
			return (await base.FindByAsync(f => f.Id == stringId, cancellationToken,
				i => i.EventsToBeProcessed, i => i.ProcessedEvents)).FirstOrDefault();
		}

		protected override void SaveChanges(TEntity entity)
		{
			base.SaveChanges(entity);
			PublishEvents(entity);
		}

		protected override async Task SaveChangesAsync(TEntity entity, CancellationToken cancellationToken = default)
		{
			await base.SaveChangesAsync(entity, cancellationToken);
			PublishEvents(entity);
		}

		private void PublishEvents(TEntity entity)
		{
			if (entity is ICqrsEntity)
			{
				var cqrsEntity = entity as ICqrsEntity;
				foreach (var @event in cqrsEntity.EventsToBeProcessed)
				{
					SetCreatedDateAndSourceId(@event, entity);
					_eventBag.Events.Add(@event);
				}

				foreach (var @event in cqrsEntity.ProcessedEvents)
					SetCreatedDateAndSourceId(@event, entity);
			}
		}

		private void SetCreatedDateAndSourceId(ICqrsEvent @event, TEntity entity)
		{
			if (!@event.CreatedDate.HasValue)
				@event.CreatedDate = _boltOnClock.Now;

			if (string.IsNullOrEmpty(@event.SourceId))
				@event.SourceId = entity.Id;
		}
	}
}
