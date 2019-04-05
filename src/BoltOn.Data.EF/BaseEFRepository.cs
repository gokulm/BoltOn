
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace BoltOn.Data.EF
{
	public abstract class BaseEFRepository<TEntity, TDbContext> : IRepository<TEntity>
		where TDbContext : DbContext
		where TEntity : class
	{
		private readonly TDbContext _dbContext;
		private readonly DbSet<TEntity> _dbSets;

		protected BaseEFRepository(IDbContextFactory dbContextFactory)
		{
			_dbContext = dbContextFactory.Get<TDbContext>();
			_dbSets = _dbContext.Set<TEntity>();
		}

		public virtual IEnumerable<TEntity> GetAll()
		{
			return _dbSets.Select(s => s).ToList();
		}

		public virtual async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default(CancellationToken))
		{
			return await _dbSets.ToListAsync(cancellationToken);
		}

		public virtual IEnumerable<TEntity> FindBy(Expression<Func<TEntity, bool>> predicate,
			params Expression<Func<TEntity, object>>[] includes)
		{
			var query = _dbSets.Where(predicate);
			if (includes.Any())
			{
				query = includes.Aggregate(query,
					(current, include) => current.Include(include));
			}

			return query.ToList();
		}

		public virtual async Task<IEnumerable<TEntity>> FindByAsync(Expression<Func<TEntity, bool>> predicate,
			CancellationToken cancellationToken = default(CancellationToken),
			params Expression<Func<TEntity, object>>[] includes)
		{
			var query = _dbSets.Where(predicate);
			if (includes != null)
			{
				query = includes.Aggregate(query,
					(current, include) => current.Include(include));
			}

			return await query.ToListAsync(cancellationToken);
		}

		public virtual TEntity Add(TEntity entity)
		{
			_dbSets.Add(entity);
			_dbContext.SaveChanges();
			return entity;
		}

		public virtual async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default(CancellationToken))
		{
			_dbSets.Add(entity);
			await _dbContext.SaveChangesAsync(cancellationToken);
			return entity;
		}

		public virtual void Update(TEntity entity)
		{
			_dbSets.Update(entity);
			_dbContext.SaveChanges();
		}

		public virtual async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default(CancellationToken))
		{
			_dbSets.Update(entity);
			await _dbContext.SaveChangesAsync(cancellationToken);
		}

		public virtual TEntity GetById<TId>(TId id)
		{
			return _dbSets.Find(id);
		}

		public virtual async Task<TEntity> GetByIdAsync<TId>(TId id)
		{
			return await _dbSets.FindAsync(id);
		}
	}
}