using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace BoltOn.Data
{
	public interface IRepository
	{
		TEntity GetById<TEntity>(object id) where TEntity : class;
		Task<TEntity> GetByIdAsync<TEntity>(object id, CancellationToken cancellationToken = default(CancellationToken))
			where TEntity : class;
		IEnumerable<TEntity> GetAll<TEntity>() where TEntity : class;
		Task<IEnumerable<TEntity>> GetAllAsync<TEntity>(CancellationToken cancellationToken = default(CancellationToken))
			where TEntity : class;
		IEnumerable<TEntity> FindBy<TEntity>(Expression<Func<TEntity, bool>> predicate,
			params Expression<Func<TEntity, object>>[] includes)
			where TEntity : class;
		Task<IEnumerable<TEntity>> FindByAsync<TEntity>(Expression<Func<TEntity, bool>> predicate,
		   CancellationToken cancellationToken = default(CancellationToken),
			params Expression<Func<TEntity, object>>[] includes)
			where TEntity : class;
		TEntity Add<TEntity>(TEntity entity) where TEntity : class;
		Task<TEntity> AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default(CancellationToken))
			where TEntity : class;
		void Update<TEntity>(TEntity entity) where TEntity : class;
		Task UpdateAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default(CancellationToken))
			where TEntity : class;
	}

	public interface IRepository<TEntity>
	{
		TEntity GetById(object id);
		Task<TEntity> GetByIdAsync(object id, CancellationToken cancellationToken = default(CancellationToken));
		IEnumerable<TEntity> GetAll();
		Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default(CancellationToken));
		IEnumerable<TEntity> FindBy(Expression<Func<TEntity, bool>> predicate,
			params Expression<Func<TEntity, object>>[] includes);
		Task<IEnumerable<TEntity>> FindByAsync(Expression<Func<TEntity, bool>> predicate,
			CancellationToken cancellationToken = default(CancellationToken),
			params Expression<Func<TEntity, object>>[] includes);
		TEntity Add(TEntity entity);
		Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default(CancellationToken));
		void Update(TEntity entity);
		Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default(CancellationToken));
	}
}
