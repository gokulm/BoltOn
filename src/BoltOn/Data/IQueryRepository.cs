using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace BoltOn.Data
{
    public interface IQueryRepository<TEntity> where TEntity : class
    {
        Task<TEntity> GetByIdAsync(object id, CancellationToken cancellationToken = default,
            params Expression<Func<TEntity, object>>[] includes);
        Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<TEntity>> FindByAsync(Expression<Func<TEntity, bool>> predicate,
            CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] includes);
    }
}
