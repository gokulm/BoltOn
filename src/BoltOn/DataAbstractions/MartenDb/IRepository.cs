using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BoltOn.DataAbstractions.MartenDb
{
	public interface IRepository<TEntity> : IQueryRepository<TEntity>
       where TEntity : class
    {
        Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);
        Task<IEnumerable<TEntity>> AddAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
        Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
        Task DeleteAsync(object id, CancellationToken cancellationToken = default);
        Task<object[]> StoreAsync(CancellationToken cancellationToken = default, params object[] objects);
    }
}
