
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

		public TEntity GetById<TEntity>(object id) where TEntity : class
		{
			return DbSets<TEntity>().Find(id);
		}

		public async Task<TEntity> GetByIdAsync<TEntity>(object id,
			CancellationToken cancellationToken = default(CancellationToken)) where TEntity : class
		{
			return await DbSets<TEntity>().FindAsync(id);
		}

		public IEnumerable<TEntity> GetAll<TEntity>() where TEntity : class
		{
			return DbSets<TEntity>().Select(s => s).ToList();
		}

		public async Task<IEnumerable<TEntity>> GetAllAsync<TEntity>(CancellationToken cancellationToken = default(CancellationToken))
			where TEntity : class
		{
			return await DbSets<TEntity>().ToListAsync(cancellationToken);
		}

		public IEnumerable<TEntity> FindBy<TEntity>(Expression<Func<TEntity, bool>> predicate,
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

		public async Task<IEnumerable<TEntity>> FindByAsync<TEntity>(Expression<Func<TEntity, bool>> predicate,
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

		public TEntity Add<TEntity>(TEntity entity) where TEntity : class
		{
			DbSets<TEntity>().Add(entity);
			DbContext.SaveChanges();
			return entity;
		}

		public async Task<TEntity> AddAsync<TEntity>(TEntity entity,
			CancellationToken cancellationToken = default(CancellationToken)) where TEntity : class
		{
			DbSets<TEntity>().Add(entity);
			await DbContext.SaveChangesAsync(cancellationToken);
			return entity;
		}

		public void Update<TEntity>(TEntity entity) where TEntity : class
		{
			DbSets<TEntity>().Attach(entity);
			DbContext.Entry(entity).State = EntityState.Modified;
			DbContext.SaveChanges();
		}

		public async Task UpdateAsync<TEntity>(TEntity entity,
			CancellationToken cancellationToken = default(CancellationToken)) where TEntity : class
		{
			DbSets<TEntity>().Attach(entity);
			DbContext.Entry(entity).State = EntityState.Modified;
			await DbContext.SaveChangesAsync(cancellationToken);
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

		public TEntity GetById(object id)
		{
			return DbSets.Find(id);
		}

		public async Task<TEntity> GetByIdAsync(object id) 
		{
			return await DbSets.FindAsync(id);
		}

		public IEnumerable<TEntity> GetAll()
		{
			return DbSets.ToList();
		}

		public async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default(CancellationToken))
		{
			return await DbSets.ToListAsync(cancellationToken);
		}

		public IEnumerable<TEntity> FindBy(Expression<Func<TEntity, bool>> predicate,
			params Expression<Func<TEntity, object>>[] includes)
		{
			var query = DbSets.Where(predicate);
			if (includes != null)
			{
				query = includes.Aggregate(query,
					(current, include) => current.Include(include));
			}

			return query.ToList();
		}

		public async Task<IEnumerable<TEntity>> FindByAsync(Expression<Func<TEntity, bool>> predicate,
			CancellationToken cancellationToken = default(CancellationToken),
			params Expression<Func<TEntity, object>>[] includes)
		{
			var query = DbSets.Where(predicate);
			if (includes != null)
			{
				query = includes.Aggregate(query,
					(current, include) => current.Include(include));
			}

			return await query.ToListAsync(cancellationToken);
		}

		public TEntity Add(TEntity entity)
		{
			DbSets.Add(entity);
			DbContext.SaveChanges();
			return entity;
		}

		public async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default(CancellationToken))
		{
			DbSets.Add(entity);
			await DbContext.SaveChangesAsync(cancellationToken);
			return entity;
		}

		public void Update(TEntity entity)
		{
			DbSets.Attach(entity);
			DbContext.Entry(entity).State = EntityState.Modified;
			DbContext.SaveChanges();
		}

		public async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default(CancellationToken))
		{
			DbSets.Attach(entity);
			DbContext.Entry(entity).State = EntityState.Modified;
			await DbContext.SaveChangesAsync(cancellationToken);
		}
	}
}