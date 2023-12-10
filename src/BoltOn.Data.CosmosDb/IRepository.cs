using System.Linq.Expressions;
using Microsoft.Azure.Cosmos;

namespace BoltOn.Data.CosmosDb
{
	public interface IRepository<TEntity>
       where TEntity : class
    {
        Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<TEntity>> FindByAsync(Expression<Func<TEntity, bool>> predicate,
            CancellationToken cancellationToken = default);
        Task<TEntity> AddAsync(TEntity entity, PartitionKey? partitionKey = null, 
            CancellationToken cancellationToken = default);

		/// <summary>
		/// The Azure Cosmos DB request size limit constrains the size of the TransactionalBatch payload to not exceed 2 MB, and the maximum execution time is 5 seconds.
		/// There's a current limit of 100 operations per TransactionalBatch to ensure the performance is as expected and within SLAs.
		/// https://learn.microsoft.com/en-us/azure/cosmos-db/nosql/transactional-batch?tabs=dotnet#limitations
		/// </summary>
		Task<IEnumerable<TEntity>> AddAsync(IEnumerable<TEntity> entities, PartitionKey partitionKey, 
            CancellationToken cancellationToken = default);
		Task UpdateAsync(TEntity entity, string id, PartitionKey? partitionKey = null, 
            CancellationToken cancellationToken = default);
        Task DeleteAsync(string id, PartitionKey partitionKey, 
            CancellationToken cancellationToken = default);
        Task<ItemResponse<TEntity>> PatchAsync(string id, PartitionKey partitionKey, 
            IEnumerable<PatchOperation> patchOperations,
            CancellationToken cancellationToken = default);    
    }
}
