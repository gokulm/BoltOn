using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
namespace BoltOn.Data
{
	public interface IRepository<TEntity> where TEntity : class
	{
		TEntity GetById(object id);
		Task<TEntity> GetByIdAsync(object id, CancellationToken cancellationToken = default);
		IEnumerable<TEntity> GetAll();
		Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
		IEnumerable<TEntity> FindBy(Expression<Func<TEntity, bool>> predicate,
			params Expression<Func<TEntity, object>>[] includes);
		Task<IEnumerable<TEntity>> FindByAsync(Expression<Func<TEntity, bool>> predicate,
			CancellationToken cancellationToken = default,
			params Expression<Func<TEntity, object>>[] includes);
		TEntity Add(TEntity entity);
		Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);
		void Update(TEntity entity);
		Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
	}
}
