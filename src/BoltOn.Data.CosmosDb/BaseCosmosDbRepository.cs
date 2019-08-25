using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Nito.AsyncEx;

namespace BoltOn.Data.CosmosDb
{
    public abstract class BaseCosmosDbRepository<TEntity, TCosmosDbContext> : IRepository<TEntity>
        where TEntity : class
        where TCosmosDbContext : BaseCosmosDbContext<TCosmosDbContext>
    {

		protected string DatabaseName { get; private set; }
		protected string CollectionName { get; private set; }
		protected DocumentClient DocumentClient { get; private set; }
		protected Uri DocumentCollectionUri { get; private set; }

		protected BaseCosmosDbRepository(TCosmosDbContext cosmosDbContext, string collectionName = null)
        {
            DatabaseName = cosmosDbContext.Options.DatabaseName;
            CollectionName = collectionName ?? typeof(TEntity).Name.Pluralize();
            DocumentClient = new DocumentClient(new Uri(cosmosDbContext.Options.Uri), cosmosDbContext.Options.AuthorizationKey);
			DocumentCollectionUri = UriFactory.CreateDocumentCollectionUri(DatabaseName, CollectionName);
		}

        public virtual TEntity Add(TEntity entity)
        {
            AsyncContext.Run(() => DocumentClient.CreateDocumentAsync(DocumentCollectionUri, entity));
            return entity;
        }

        public virtual async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default(CancellationToken))
        {
            await DocumentClient.CreateDocumentAsync(DocumentCollectionUri, entity);
            return entity;
        }

        public virtual IEnumerable<TEntity> FindBy(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includes)
        {
            var query = DocumentClient.CreateDocumentQuery<TEntity>(DocumentCollectionUri)
                .Where(predicate)
                .AsDocumentQuery();

            return AsyncContext.Run(() => GetResultsFromDocumentQuery(query));
        }

        public virtual async Task<IEnumerable<TEntity>> FindByAsync(Expression<Func<TEntity, bool>> predicate,
            CancellationToken cancellationToken = default(CancellationToken), params Expression<Func<TEntity, object>>[] includes)
        {
            var query = DocumentClient.CreateDocumentQuery<TEntity>(DocumentCollectionUri)
               .Where(predicate)
               .AsDocumentQuery();

            return await GetResultsFromDocumentQuery(query);
        }

        public virtual IEnumerable<TEntity> GetAll()
        {
            var query = DocumentClient.CreateDocumentQuery<TEntity>
                (DocumentCollectionUri)
                .AsDocumentQuery();

            return AsyncContext.Run(() => GetResultsFromDocumentQuery(query));
        }

        public virtual async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var query = DocumentClient.CreateDocumentQuery<TEntity>
                (DocumentCollectionUri)
                .AsDocumentQuery();

            return await GetResultsFromDocumentQuery(query);
        }

        public virtual TEntity GetById<TId>(TId id)
        {
            var document = AsyncContext.Run(() => DocumentClient.ReadDocumentAsync<TEntity>(GetDocumentUri(id.ToString())));
            return document.Document;
        }

        public virtual async Task<TEntity> GetByIdAsync<TId>(TId id)
        {
            var document = await DocumentClient.ReadDocumentAsync<TEntity>(GetDocumentUri(id.ToString()));
            return document.Document;
        }

        public virtual void Update(TEntity entity)
        {
            AsyncContext.Run(() => DocumentClient.UpsertDocumentAsync(DocumentCollectionUri, entity));
        }

        public virtual async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default(CancellationToken))
        {
            await DocumentClient.UpsertDocumentAsync(DocumentCollectionUri, entity);
        }

        protected Uri GetDocumentUri(string id)
        {
            return UriFactory.CreateDocumentUri(DatabaseName, CollectionName, id);
        }

        protected async Task<IEnumerable<TEntity>> GetResultsFromDocumentQuery(IDocumentQuery<TEntity> query)
        {
            List<TEntity> results = new List<TEntity>();
            while (query.HasMoreResults)
            {
                results.AddRange(await query.ExecuteNextAsync<TEntity>());
            }
            return results;
        }
    }
}
