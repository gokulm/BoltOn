using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
namespace BoltOn.Data
{
	public interface IRepository<TEntity> where TEntity : class
	{
		TEntity GetById(object id, object options = null);
		Task<TEntity> GetByIdAsync(object id, object options = null, CancellationToken cancellationToken = default);
		IEnumerable<TEntity> GetAll(object options = null);
		Task<IEnumerable<TEntity>> GetAllAsync(object options = null, CancellationToken cancellationToken = default);
		IEnumerable<TEntity> FindBy(Expression<Func<TEntity, bool>> predicate, object options = null);
		Task<IEnumerable<TEntity>> FindByAsync(Expression<Func<TEntity, bool>> predicate, object options = null,
			CancellationToken cancellationToken = default);
		TEntity Add(TEntity entity, object options = null);
		Task<TEntity> AddAsync(TEntity entity, object options = null, CancellationToken cancellationToken = default);
		void Update(TEntity entity, object options = null);
		Task UpdateAsync(TEntity entity, object options = null, CancellationToken cancellationToken = default);
		void Delete(TEntity entity, object options = null);
		Task DeleteAsync(TEntity entity, object options = null, CancellationToken cancellationToken = default);
	}
}
