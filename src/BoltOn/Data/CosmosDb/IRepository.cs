using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System;

namespace BoltOn.Data.CosmosDb
{
    public interface IRepository<TEntity>
       where TEntity : class
    {
        Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<TEntity>> FindByAsync(Expression<Func<TEntity, bool>> predicate,
            CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] includes);
        Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);
        Task UpdateAsync(TEntity entity, string id, CancellationToken cancellationToken = default);
        Task DeleteAsync(string id, string partitionKey, CancellationToken cancellationToken = default);
    }
}
