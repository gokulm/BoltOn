using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BoltOn.Bus;
using BoltOn.Cqrs;
using Microsoft.EntityFrameworkCore;

namespace BoltOn.Data.EF
{
	public class CqrsRepository<TEntity, TDbContext> : Repository<TEntity, TDbContext>
		where TDbContext : DbContext
		where TEntity : BaseCqrsEntity
	{
		private readonly IAppServiceBus _bus;

		public CqrsRepository(TDbContext dbContext,
			IAppServiceBus bus) : base(dbContext)
		{
			_bus = bus;
		}

		protected override async Task SaveChangesAsync(TEntity entity, CancellationToken cancellationToken = default)
		{
			await SaveChangesAsync(new[] { entity }, cancellationToken);
		}

		protected override async Task SaveChangesAsync(IEnumerable<TEntity> entities,
			CancellationToken cancellationToken = default)
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
				_bus.PublishAsync(e);
			});
		}
	}
}