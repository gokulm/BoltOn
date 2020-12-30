using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Newtonsoft.Json;

namespace BoltOn.Data.CosmosDb
{
	public class Repository<TEntity, TCosmosDbOptions> : IRepository<TEntity> where TEntity : class
		where TCosmosDbOptions : BaseCosmosDbOptions
	{
		protected string DatabaseName { get; }
		protected string CollectionName { get; }
		protected DocumentClient DocumentClient { get; }
		protected Uri DocumentCollectionUri { get; }

		public Repository(TCosmosDbOptions options, string collectionName = null)
		{
			DatabaseName = options.DatabaseName;
			CollectionName = collectionName ?? typeof(TEntity).Name.Pluralize();
			DocumentClient = new DocumentClient(new Uri(options.Uri), options.AuthorizationKey,
				new JsonSerializerSettings
				{
					TypeNameHandling = TypeNameHandling.Auto
				});
			DocumentCollectionUri = UriFactory.CreateDocumentCollectionUri(DatabaseName, CollectionName);
		}

		public virtual async Task<TEntity> GetByIdAsync(object id, RequestOptions options = null, CancellationToken cancellationToken = default)
		{
			try
			{
				return await DocumentClient.ReadDocumentAsync<TEntity>(GetDocumentUri(id.ToString()),
					options, cancellationToken);
			}
			catch (DocumentClientException e)
			{
				if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
				{
					return null;
				}
				throw;
			}
		}

		public virtual async Task<IEnumerable<TEntity>> FindByAsync(Expression<Func<TEntity, bool>> predicate,
			FeedOptions options = null,
			CancellationToken cancellationToken = default)
		{
			var orderedQueryable = DocumentClient.CreateDocumentQuery<TEntity>(DocumentCollectionUri, options);
			var query = orderedQueryable
			   .Where(predicate)
			   .AsDocumentQuery();

			return await GetResultsFromDocumentQuery(query, cancellationToken);
		}

		public virtual async Task<IEnumerable<TEntity>> GetAllAsync(FeedOptions options = null, CancellationToken cancellationToken = default)
		{
			var orderedQueryable = DocumentClient.CreateDocumentQuery<TEntity>(DocumentCollectionUri, options);
			return await GetResultsFromDocumentQuery(orderedQueryable.AsDocumentQuery(), cancellationToken);
		}

		public virtual async Task<TEntity> AddAsync(TEntity entity, RequestOptions options = null, CancellationToken cancellationToken = default)
		{
			PublishEvents(entity);
			await DocumentClient.CreateDocumentAsync(DocumentCollectionUri, entity, options, cancellationToken: cancellationToken);
			return entity;
		}

		public virtual async Task<IEnumerable<TEntity>> AddAsync(IEnumerable<TEntity> entities, RequestOptions options = null, CancellationToken cancellationToken = default)
		{
			// todo: find a way to add entities as a collection instead of iterating every entity
			foreach (var entity in entities)
			{
				await AddAsync(entity, options, cancellationToken);
			}
			return entities;
		}

		public virtual async Task UpdateAsync(TEntity entity, RequestOptions options = null, CancellationToken cancellationToken = default)
		{
			PublishEvents(entity);
			await DocumentClient.UpsertDocumentAsync(DocumentCollectionUri, entity, options, cancellationToken: cancellationToken);
		}

		public virtual async Task DeleteAsync(object id, RequestOptions options = null, CancellationToken cancellationToken = default)
		{
			await DocumentClient.DeleteDocumentAsync(GetDocumentUri(id.ToString()), options);
		}

		protected Uri GetDocumentUri(string id)
		{
			return UriFactory.CreateDocumentUri(DatabaseName, CollectionName, id);
		}

		protected async Task<IEnumerable<TEntity>> GetResultsFromDocumentQuery(IDocumentQuery<TEntity> query, CancellationToken cancellationToken = default)
		{
			List<TEntity> results = new List<TEntity>();
			while (query.HasMoreResults)
			{
				results.AddRange(await query.ExecuteNextAsync<TEntity>(cancellationToken));
			}
			return results;
		}

		protected virtual void PublishEvents(TEntity entity)
		{
		}
	}
}