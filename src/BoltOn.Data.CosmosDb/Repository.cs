using System.Linq.Expressions;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

namespace BoltOn.Data.CosmosDb
{
	public class Repository<TEntity> : IRepository<TEntity>
		where TEntity : class
	{
		public Container Container { get; private set; }

		public Repository(CosmosClient cosmosClient, string? containerName = null)
		{
			var clientOptions = cosmosClient.ClientOptions;
			var dbContainerName = containerName ?? typeof(TEntity).Name.Pluralize();
			Container = cosmosClient.GetContainer(clientOptions.ApplicationName, dbContainerName);
		}

		public virtual async Task<TEntity> AddAsync(TEntity entity, PartitionKey? partitionKey = null, 
			CancellationToken cancellationToken = default)
		{
			var response = await Container.CreateItemAsync(entity, partitionKey, cancellationToken: cancellationToken);
			return response.Resource;
		}

		public virtual async Task<IEnumerable<TEntity>> FindByAsync(Expression<Func<TEntity, bool>> predicate,
			CancellationToken cancellationToken = default)
		{
			var query = Container.GetItemLinqQueryable<TEntity>()
							.Where(predicate)
							.ToFeedIterator();

			var entities = new List<TEntity>();
			while (query.HasMoreResults)
			{
				var response = await query.ReadNextAsync(cancellationToken);
				entities.AddRange(response);
			}

			return entities;
		}

		public virtual async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
		{
			var query = Container.GetItemLinqQueryable<TEntity>()
					.ToFeedIterator();

			var entities = new List<TEntity>();
			while (query.HasMoreResults)
			{
				var response = await query.ReadNextAsync(cancellationToken);
				entities.AddRange(response);
			}

			return entities;
		}

		public virtual async Task UpdateAsync(TEntity entity, string id, 
			PartitionKey? partitionKey = null, CancellationToken cancellationToken = default)
		{
			await Container.ReplaceItemAsync(entity, id, partitionKey, cancellationToken: cancellationToken);
		}

		public virtual async Task DeleteAsync(string id, PartitionKey partitionKey, 
			CancellationToken cancellationToken = default)
		{
			await Container.DeleteItemAsync<TEntity>(id, partitionKey, cancellationToken: cancellationToken);
		}

		public async Task<IEnumerable<TEntity>> AddAsync(IEnumerable<TEntity> entities, PartitionKey partitionKey,
			CancellationToken cancellationToken = default)
		{
			var batch = Container.CreateTransactionalBatch(partitionKey);

			foreach (var entity in entities)
			{
				batch.CreateItem<TEntity>(entity);
			}

			using TransactionalBatchResponse response = await batch.ExecuteAsync(cancellationToken);
			var results = new List<TEntity>();
			if (response.IsSuccessStatusCode)
			{
				for (int i = 0; i < entities.Count(); i++)
				{
					var entityResponse = response.GetOperationResultAtIndex<TEntity>(i);
					results.Add(entityResponse.Resource);
				}
			}
			return results;
		}
	}
}