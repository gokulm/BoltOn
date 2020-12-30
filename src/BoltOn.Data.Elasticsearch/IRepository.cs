using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nest;

namespace BoltOn.Data.Elasticsearch
{
    public interface IRepository<TEntity>
        where TEntity : class
    {
        Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);
        Task<IEnumerable<TEntity>> AddAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
        Task DeleteAsync(object id, CancellationToken cancellationToken = default);
        Task<IEnumerable<TEntity>> FindByAsync(SearchRequest searchRequest = null, CancellationToken cancellationToken = default);
        Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<TEntity> GetByIdAsync(object id, CancellationToken cancellationToken = default);
        Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    }
}