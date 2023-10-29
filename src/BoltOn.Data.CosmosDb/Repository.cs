using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using BoltOn.Data.CosmosDb;

namespace BoltOn.Data.CosmosDb
{
	public class Repository<TEntity> : IRepository<TEntity>
		where TEntity : class
	{
		private readonly Container _container;

		public Repository(CosmosClient cosmosClient, string? containerName = null)
		{
			var clientOptions = cosmosClient.ClientOptions;
			var dbContainerName = containerName ?? typeof(TEntity).Name.Pluralize();
			_container = cosmosClient.GetContainer(clientOptions.ApplicationName, dbContainerName);
		}

		public async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
		{
			var response = await _container.CreateItemAsync(entity, cancellationToken: cancellationToken);
			return response.Resource;
		}

		public async Task<IEnumerable<TEntity>> FindByAsync(Expression<Func<TEntity, bool>> predicate,
			CancellationToken cancellationToken = default)
		{
			var query = _container.GetItemLinqQueryable<TEntity>()
							.Where(predicate)
							.ToFeedIterator();

			var entities = new List<TEntity>();
			while (query.HasMoreResults)
			{
				var response = await query.ReadNextAsync();
				entities.AddRange(response);
			}

			return entities;
		}

		public async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
		{
			var query = _container.GetItemLinqQueryable<TEntity>()
					.ToFeedIterator();

			var entities = new List<TEntity>();
			while (query.HasMoreResults)
			{
				var response = await query.ReadNextAsync();
				entities.AddRange(response);
			}

			return entities;
		}

		public async Task UpdateAsync(TEntity entity, string id, CancellationToken cancellationToken = default)
		{
			await _container.ReplaceItemAsync(entity, id, cancellationToken: cancellationToken);
		}

		public async Task DeleteAsync(string id, string partitionKey, CancellationToken cancellationToken = default)
		{
			await _container.DeleteItemAsync<TEntity>(id, new PartitionKey(partitionKey), cancellationToken: cancellationToken);
		}
	}
}