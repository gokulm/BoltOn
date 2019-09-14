using System.Threading;
using System.Threading.Tasks;
using BoltOn.Cqrs;
using Microsoft.EntityFrameworkCore;

namespace BoltOn.Data.EF
{
    public abstract class BaseEFCqrsRepository<TEntity, TDbContext> : BaseEFRepository<TEntity, TDbContext>, IRepository<TEntity>
       where TDbContext : DbContext
       where TEntity : class
    {
        private readonly EventBag _eventBag;

        protected BaseEFCqrsRepository(IDbContextFactory dbContextFactory, EventBag eventBag) : base(dbContextFactory)
        {
            _eventBag = eventBag;
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
                foreach (var @event in cqrsEntity.Events)
                {
                    _eventBag.Events.Add(@event);
                }
            }
        }
    }
}
