
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace BoltOn.Data.EF
{
	public abstract class BaseEFRepository<TDbContext> : IRepository
		where TDbContext : DbContext
	{
		protected TDbContext DbContext { get; private set; }

		protected DbSet<TEntity> DbSets<TEntity>() where TEntity : class
		{
			return DbContext.Set<TEntity>();
		}

		protected BaseEFRepository(IDbContextFactory dbContextFactory)
		{
			DbContext = dbContextFactory.Get<TDbContext>();
		}

		public virtual IEnumerable<TEntity> GetAll<TEntity>() where TEntity : class
		{
			return DbSets<TEntity>().Select(s => s).ToList();
		}

		public virtual async Task<IEnumerable<TEntity>> GetAllAsync<TEntity>(CancellationToken cancellationToken = default(CancellationToken))
			where TEntity : class
		{
			return await DbSets<TEntity>().ToListAsync(cancellationToken);
		}

		public virtual IEnumerable<TEntity> FindBy<TEntity>(Expression<Func<TEntity, bool>> predicate,
			params Expression<Func<TEntity, object>>[] includes)
			where TEntity : class
		{
			var query = DbSets<TEntity>().Where(predicate);
			if (includes.Any())
			{
				query = includes.Aggregate(query,
					(current, include) => current.Include(include));
			}

			return query.ToList();
		}

		public virtual async Task<IEnumerable<TEntity>> FindByAsync<TEntity>(Expression<Func<TEntity, bool>> predicate,
			CancellationToken cancellationToken = default(CancellationToken),
			params Expression<Func<TEntity, object>>[] includes)
			where TEntity : class
		{
			var query = DbSets<TEntity>().Where(predicate);
			if (includes != null)
			{
				query = includes.Aggregate(query,
					(current, include) => current.Include(include));
			}

			return await query.ToListAsync(cancellationToken);
		}

		// APPLIES to Add and Update
		// in case if records should not be added or updated when TrackingBehavior is NoTracking, we can 
		// check the behavior in Add and Update methods, and not call DbSets.Add or DbSets.Update and SaveChanges

		public virtual TEntity Add<TEntity>(TEntity entity) where TEntity : class
		{
			DbSets<TEntity>().Add(entity);
			DbContext.SaveChanges();
			return entity;
		}

		public virtual async Task<TEntity> AddAsync<TEntity>(TEntity entity,
			CancellationToken cancellationToken = default(CancellationToken)) where TEntity : class
		{
			DbSets<TEntity>().Add(entity);
			await DbContext.SaveChangesAsync(cancellationToken);
			return entity;
		}

		public virtual void Update<TEntity>(TEntity entity) where TEntity : class
		{
			DbSets<TEntity>().Update(entity);
			DbContext.SaveChanges();
		}

		public virtual async Task UpdateAsync<TEntity>(TEntity entity,
			CancellationToken cancellationToken = default(CancellationToken)) where TEntity : class
		{
			DbSets<TEntity>().Update(entity);
			await DbContext.SaveChangesAsync(cancellationToken);
		}

		public TEntity GetById<TEntity, TId>(TId id) where TEntity : class
		{
			return DbSets<TEntity>().Find(id);
		}

		public async Task<TEntity> GetByIdAsync<TEntity, TId>(TId id) where TEntity : class
		{
			return await DbSets<TEntity>().FindAsync(id);
		}
	}

	public abstract class BaseEFRepository<TEntity, TDbContext> : BaseEFRepository<TDbContext>, IRepository<TEntity>
		where TDbContext : DbContext
		where TEntity : class
	{
		protected DbSet<TEntity> DbSets => DbSets<TEntity>();

		protected BaseEFRepository(IDbContextFactory dbContextFactory) : base(dbContextFactory)
		{
		}

		public virtual IEnumerable<TEntity> GetAll()
		{
			return GetAll<TEntity>();
		}

		public virtual async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default(CancellationToken))
		{
			return await GetAllAsync<TEntity>(cancellationToken);
		}

		public virtual IEnumerable<TEntity> FindBy(Expression<Func<TEntity, bool>> predicate,
			params Expression<Func<TEntity, object>>[] includes)
		{
			return FindBy<TEntity>(predicate, includes);
		}

		public virtual async Task<IEnumerable<TEntity>> FindByAsync(Expression<Func<TEntity, bool>> predicate,
			CancellationToken cancellationToken = default(CancellationToken),
			params Expression<Func<TEntity, object>>[] includes)
		{
			return await FindByAsync<TEntity>(predicate, cancellationToken, includes);
		}

		public virtual TEntity Add(TEntity entity)
		{
			return Add<TEntity>(entity);
		}

		public virtual async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default(CancellationToken))
		{
			return await AddAsync<TEntity>(entity, cancellationToken);
		}

		public virtual void Update(TEntity entity)
		{
			Update<TEntity>(entity);
		}

		public virtual async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default(CancellationToken))
		{
			await UpdateAsync<TEntity>(entity, cancellationToken);
		}

		public virtual TEntity GetById<TId>(TId id)
		{
			return GetById<TEntity, TId>(id);
		}

		public virtual async Task<TEntity> GetByIdAsync<TId>(TId id)
		{
			return await GetByIdAsync<TEntity, TId>(id);
		}
	}
}