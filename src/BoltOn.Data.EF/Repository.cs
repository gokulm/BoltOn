using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BoltOn.DataAbstractions.EF;


namespace BoltOn.Data.EF
{
	public class Repository<TEntity, TDbContext> : IRepository<TEntity>
		where TDbContext : DbContext
		where TEntity : class
	{
		protected TDbContext DbContext { get; set; }
		protected DbSet<TEntity> DbSets { get; }

		public Repository(TDbContext dbContext)
		{
			DbContext = dbContext;
			DbSets = DbContext.Set<TEntity>();
		}

		public virtual async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
		{
			DbSets.Add(entity);
			await SaveChangesAsync(entity, cancellationToken);
			return entity;
		}

		public virtual async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
		{
			if (!DbContext.ChangeTracker.Entries<TEntity>().Select(s => s.Entity).Contains(entity))
				DbSets.Update(entity);
			await SaveChangesAsync(entity, cancellationToken);
		}

		public virtual async Task DeleteAsync(object id, CancellationToken cancellationToken = default)
		{
			// todo: delete without loading
			var entity = await GetByIdAsync(id, cancellationToken);
			DbSets.Remove(entity);
			await SaveChangesAsync(entity, cancellationToken);
		}

		public virtual async Task<IEnumerable<TEntity>> AddAsync(IEnumerable<TEntity> entities,
			CancellationToken cancellationToken = default)
		{
			await DbSets.AddRangeAsync(entities);
			await SaveChangesAsync(entities, cancellationToken);
			return entities;
		}

		protected virtual async Task SaveChangesAsync(TEntity entity, CancellationToken cancellationToken = default)
		{
			cancellationToken.ThrowIfCancellationRequested();
			await SaveChangesAsync(new[] { entity }, cancellationToken);
		}

		protected virtual async Task SaveChangesAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
		{
			cancellationToken.ThrowIfCancellationRequested();
			await DbContext.SaveChangesAsync(cancellationToken);
		}

		public virtual async Task<TEntity> GetByIdAsync(object id, CancellationToken cancellationToken = default,
			params Expression<Func<TEntity, object>>[] includes)
		{
			cancellationToken.ThrowIfCancellationRequested();
			return await DbSets.FindAsync(id);
		}

		public virtual async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
		{
			cancellationToken.ThrowIfCancellationRequested();
			return await DbSets.ToListAsync(cancellationToken);
		}

		public virtual async Task<IEnumerable<TEntity>> FindByAsync(Expression<Func<TEntity, bool>> predicate,
			CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] includes)
		{
			cancellationToken.ThrowIfCancellationRequested();
			var query = DbSets.Where(predicate);
			if (includes != null)
			{
				query = includes.Aggregate(query,
				(current, include) => current.Include(include));
			}

			return await query.ToListAsync(cancellationToken);
		}
	}
}
