using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace BoltOn.Data
{
    public interface IRepository<TEntity> where TEntity : class
    {
        void Init<TOptions>(TOptions options);
        TEntity GetById<TId>(TId id);
        Task<TEntity> GetByIdAsync<TId>(TId id);
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
