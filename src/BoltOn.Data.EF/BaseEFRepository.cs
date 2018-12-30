
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
		private readonly TDbContext _dbContext;

		protected DbSet<TEntity> DbSets<TEntity>() where TEntity : class
		{
			return _dbContext.Set<TEntity>();
		}

		protected BaseEFRepository(IDbContextFactory dbContextFactory)
		{
			_dbContext = dbContextFactory.Get<TDbContext>();
		}

		public TEntity GetById<TEntity>(object id) where TEntity : class
		{
			return DbSets<TEntity>().Find(id);
		}

		public async Task<TEntity> GetByIdAsync<TEntity>(object id,
			CancellationToken cancellationToken = default(CancellationToken)) where TEntity : class
		{
			return await DbSets<TEntity>().FindAsync(id, cancellationToken);
		}

		public IEnumerable<TEntity> GetAll<TEntity>() where TEntity : class
		{
			return DbSets<TEntity>().ToList();
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
			if (includes != null)
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

		public void Add<TEntity>(TEntity entity) where TEntity : class
		{
			DbSets<TEntity>().Add(entity);
			_dbContext.SaveChanges();
		}

		public async Task AddAsync<TEntity>(TEntity entity,
			CancellationToken cancellationToken = default(CancellationToken)) where TEntity : class
		{
			DbSets<TEntity>().Add(entity);
			await _dbContext.SaveChangesAsync(cancellationToken);
		}

		public void Update<TEntity>(TEntity entity) where TEntity : class
		{
			DbSets<TEntity>().Attach(entity);
			_dbContext.Entry(entity).State = EntityState.Modified;
			_dbContext.SaveChanges();
		}

		public async Task UpdateAsync<TEntity>(TEntity entity,
			CancellationToken cancellationToken = default(CancellationToken)) where TEntity : class
		{
			DbSets<TEntity>().Attach(entity);
			_dbContext.Entry(entity).State = EntityState.Modified;
			await _dbContext.SaveChangesAsync(cancellationToken);
		}
	}

	public abstract class BaseEFRepository<TEntity, TDbContext> : BaseEFRepository<TDbContext>, IRepository<TEntity>
		where TDbContext : DbContext
		where TEntity : class
	{
		private readonly TDbContext _dbContext;
		protected DbSet<TEntity> DbSets => DbSets<TEntity>();

		protected BaseEFRepository(IDbContextFactory dbContextFactory) : base(dbContextFactory)
		{
			_dbContext = dbContextFactory.Get<TDbContext>();
		}

		public TEntity GetById(object id)
		{
			return DbSets.Find(id);
		}

		public async Task<TEntity> GetByIdAsync(object id,
			CancellationToken cancellationToken = default(CancellationToken))
		{
			return await DbSets.FindAsync(id, cancellationToken);
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

		public void Add(TEntity entity)
		{
			DbSets.Add(entity);
			_dbContext.SaveChanges();
		}

		public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default(CancellationToken))
		{
			DbSets.Add(entity);
			await _dbContext.SaveChangesAsync(cancellationToken);
		}

		public void Update(TEntity entity)
		{
			DbSets.Attach(entity);
			_dbContext.Entry(entity).State = EntityState.Modified;
			_dbContext.SaveChanges();
		}

		public async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default(CancellationToken))
		{
			DbSets.Attach(entity);
			_dbContext.Entry(entity).State = EntityState.Modified;
			await _dbContext.SaveChangesAsync(cancellationToken);
		}
	}
}