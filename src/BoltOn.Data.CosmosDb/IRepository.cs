using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Client;

namespace BoltOn.Data.CosmosDb
{
    public interface IRepository<TEntity> where TEntity : class
    {
        Task<TEntity> AddAsync(TEntity entity, RequestOptions options = null,
			CancellationToken cancellationToken = default);
        Task<IEnumerable<TEntity>> AddAsync(IEnumerable<TEntity> entities, RequestOptions options = null,
			CancellationToken cancellationToken = default);
        Task DeleteAsync(object id, RequestOptions options = null, CancellationToken cancellationToken = default);
        Task<IEnumerable<TEntity>> FindByAsync(Expression<Func<TEntity, bool>> predicate, FeedOptions options = null,
			CancellationToken cancellationToken = default);
        Task<IEnumerable<TEntity>> GetAllAsync(FeedOptions options = null, CancellationToken cancellationToken = default);
        Task<TEntity> GetByIdAsync(object id, RequestOptions options = null, CancellationToken cancellationToken = default);
        Task UpdateAsync(TEntity entity, RequestOptions options = null, CancellationToken cancellationToken = default);
    }
}