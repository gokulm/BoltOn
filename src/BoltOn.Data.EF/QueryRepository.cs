using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace BoltOn.Data.EF
{
	public class QueryRepository<TEntity, TDbContext> : IQueryRepository<TEntity>
		where TDbContext : DbContext
		where TEntity : class
	{
		protected TDbContext DbContext { get; set; }
		protected DbSet<TEntity> DbSets { get; }

		public QueryRepository(TDbContext dbContext)
		{
			DbContext = dbContext;
			DbSets = DbContext.Set<TEntity>();
		}

		public async Task<TEntity> GetByIdAsync(object id, CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] includes)
		{
			try
			{
				cancellationToken.ThrowIfCancellationRequested();
				DbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
				return await DbSets.FindAsync(id);
			}
			finally
			{
				DbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;
			}
		}

		public async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
		{
			try
			{
				cancellationToken.ThrowIfCancellationRequested();
				DbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
				return await DbSets.ToListAsync(cancellationToken);
			}
			finally
			{
				DbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;
			}
		}

		public async Task<IEnumerable<TEntity>> FindByAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] includes)
		{
			try
			{
				cancellationToken.ThrowIfCancellationRequested();
				DbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
				var query = DbSets.Where(predicate);
				if (includes != null)
				{
					query = includes.Aggregate(query,
					(current, include) => current.Include(include));
				}

				return await query.ToListAsync(cancellationToken);
			}
			finally
			{
				DbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;
			}
		}
	}
}
