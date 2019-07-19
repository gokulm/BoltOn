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
        protected readonly string _databaseName;
        protected readonly string _collectionName;
        protected readonly DocumentClient _client;
        private readonly Uri _documentCollectionUri;

        public BaseCosmosDbRepository(TCosmosDbContext cosmosDbContext, string collectionName = null)
        {
            _databaseName = cosmosDbContext.DatabaseName;
            _collectionName = collectionName ?? typeof(TEntity).Name.Pluralize();
            _client = cosmosDbContext.DocumentClient;
            _documentCollectionUri = GetDocumentCollectionUri();
        }

        public virtual TEntity Add(TEntity entity)
        {
            AsyncContext.Run(() => _client.CreateDocumentAsync(_documentCollectionUri, entity));
            return entity;
        }

        public virtual async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default(CancellationToken))
        {
            await _client.CreateDocumentAsync(_documentCollectionUri, entity);
            return entity;
        }

        public virtual IEnumerable<TEntity> FindBy(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includes)
        {
            var query = _client.CreateDocumentQuery<TEntity>(_documentCollectionUri)
                .Where(predicate)
                .AsDocumentQuery();

            return AsyncContext.Run(() => GetResultsFromDocumentQuery(query));
        }

        public virtual async Task<IEnumerable<TEntity>> FindByAsync(Expression<Func<TEntity, bool>> predicate,
            CancellationToken cancellationToken = default(CancellationToken), params Expression<Func<TEntity, object>>[] includes)
        {
            var query = _client.CreateDocumentQuery<TEntity>(_documentCollectionUri)
               .Where(predicate)
               .AsDocumentQuery();

            return await GetResultsFromDocumentQuery(query);
        }

        public virtual IEnumerable<TEntity> GetAll()
        {
            var query = _client.CreateDocumentQuery<TEntity>
                (_documentCollectionUri)
                .AsDocumentQuery();

            return AsyncContext.Run(() => GetResultsFromDocumentQuery(query));
        }

        public virtual async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var query = _client.CreateDocumentQuery<TEntity>
                (_documentCollectionUri)
                .AsDocumentQuery();

            return await GetResultsFromDocumentQuery(query);
        }

        public virtual TEntity GetById<TId>(TId id)
        {
            var document = AsyncContext.Run(() => _client.ReadDocumentAsync<TEntity>(GetDocumentUri(id.ToString())));
            return document.Document;
        }

        public virtual async Task<TEntity> GetByIdAsync<TId>(TId id)
        {
            var document = await _client.ReadDocumentAsync<TEntity>(GetDocumentUri(id.ToString()));
            return document.Document;
        }

        public virtual void Update(TEntity entity)
        {
            AsyncContext.Run(() => _client.UpsertDocumentAsync(_documentCollectionUri, entity));
        }

        public virtual async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default(CancellationToken))
        {
            await _client.UpsertDocumentAsync(_documentCollectionUri, entity);
        }

        protected Uri GetDocumentCollectionUri()
        {
            return UriFactory.CreateDocumentCollectionUri(_databaseName, _collectionName);
        }

        protected Uri GetDocumentUri(string id)
        {
            return UriFactory.CreateDocumentUri(_databaseName, _collectionName, id);
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
