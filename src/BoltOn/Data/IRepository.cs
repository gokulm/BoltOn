using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
namespace BoltOn.Data
{
	public interface IRepository<TEntity> where TEntity : class
	{
		Task<TEntity> GetByIdAsync(object id, object options = null, CancellationToken cancellationToken = default);
		Task<IEnumerable<TEntity>> GetAllAsync(object options = null, CancellationToken cancellationToken = default);
		Task<IEnumerable<TEntity>> FindByAsync(Expression<Func<TEntity, bool>> predicate, object options = null,
			CancellationToken cancellationToken = default);
		Task<TEntity> AddAsync(TEntity entity, object options = null, CancellationToken cancellationToken = default);
		Task<IEnumerable<TEntity>> AddAsync(IEnumerable<TEntity> entities, object options = null, CancellationToken cancellationToken = default);
		Task UpdateAsync(TEntity entity, object options = null, CancellationToken cancellationToken = default);
		Task DeleteAsync(TEntity entity, object options = null, CancellationToken cancellationToken = default);
	}
}
