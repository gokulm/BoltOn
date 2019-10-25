
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace BoltOn.Data.EF
{
	public class Repository<TEntity, TDbContext> : IRepository<TEntity>
		where TDbContext : DbContext
		where TEntity : class
	{
		private readonly TDbContext _dbContext;
		private readonly DbSet<TEntity> _dbSets;

		public Repository(IDbContextFactory dbContextFactory)
		{
			_dbContext = dbContextFactory.Get<TDbContext>();
			_dbSets = _dbContext.Set<TEntity>();
		}

		public virtual IEnumerable<TEntity> GetAll()
		{
			return _dbSets.Select(s => s).ToList();
		}

		public virtual async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
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
			CancellationToken cancellationToken = default,
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
			SaveChanges(entity);
			return entity;
		}

		public virtual async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
		{
			_dbSets.Add(entity);
			await SaveChangesAsync(entity, cancellationToken);
			return entity;
		}

		public virtual void Update(TEntity entity)
		{
			_dbSets.Update(entity);
			SaveChanges(entity);
		}

		public virtual async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
		{
			_dbSets.Update(entity);
			await SaveChangesAsync(entity, cancellationToken);
		}

		public virtual TEntity GetById(object id)
		{
			return _dbSets.Find(id);
		}

		public virtual async Task<TEntity> GetByIdAsync(object id, object options = null, CancellationToken cancellationToken = default)
		{
			return await _dbSets.FindAsync(id);
		}

		protected virtual void SaveChanges(TEntity entity)
		{
			_dbContext.SaveChanges();
		}

		protected virtual async Task SaveChangesAsync(TEntity entity, CancellationToken cancellationToken = default)
		{
			await _dbContext.SaveChangesAsync(cancellationToken);
		}
	}
}