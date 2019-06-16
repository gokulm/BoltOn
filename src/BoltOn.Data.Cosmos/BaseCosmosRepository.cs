﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;

namespace BoltOn.Data.Cosmos
{
    public abstract class BaseCosmosRepository<TEntity, TCosmosContext> : IRepository<TEntity>
        where TEntity : class
        where TCosmosContext : BaseCosmosContext
    {
        private readonly TCosmosContext _cosmosContext;
        protected readonly string _databaseName;
        protected readonly string _collectionName;
        protected readonly DocumentClient _client;
        protected RequestOptions RequestOptions { get; set; }
        protected FeedOptions FeedOptions { get; set; }

        public BaseCosmosRepository(ICosmosContextFactory cosmosContextFactory, string collectionName = null)
        {
            _cosmosContext = cosmosContextFactory.Get<TCosmosContext>();
            _databaseName = _cosmosContext.CosmosSetting.DatabaseName;
            _collectionName = collectionName ?? typeof(TEntity).Name.Pluralize();
            _client = new DocumentClient(new Uri(_cosmosContext.CosmosSetting.Uri), _cosmosContext.CosmosSetting.AuthorizationKey);
        }

        public virtual TEntity Add(TEntity entity)
        {
            _client.CreateDocumentAsync(GetDocumentCollectionUri(), entity, RequestOptions).Wait();
            return entity;
        }

        public virtual TEntity AddCosmosEntity<TCosmosOptions>(TEntity entity, TCosmosOptions requestOptions = null) where TCosmosOptions : class
        {
            if (requestOptions == null)
                _client.CreateDocumentAsync(GetDocumentCollectionUri(), entity).Wait();
            else
            {
                var options = requestOptions as RequestOptions;
                _client.CreateDocumentAsync(GetDocumentCollectionUri(), entity, options).Wait();
            }
            return entity;
        }

        public virtual async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default(CancellationToken))
        {
            await _client.CreateDocumentAsync(GetDocumentCollectionUri(), entity, RequestOptions);
            return entity;
        }

        public virtual IEnumerable<TEntity> FindBy(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includes)
        {
            var query = _client.CreateDocumentQuery<TEntity>(GetDocumentCollectionUri(), FeedOptions)
                .Where(predicate)
                .AsDocumentQuery();

            return GetResultsFromDocumentQuery(query).Result;
        }

        public virtual async Task<IEnumerable<TEntity>> FindByAsync(Expression<Func<TEntity, bool>> predicate,
            CancellationToken cancellationToken = default(CancellationToken), params Expression<Func<TEntity, object>>[] includes)
        {
            var query = _client.CreateDocumentQuery<TEntity>(GetDocumentCollectionUri(), FeedOptions)
               .Where(predicate)
               .AsDocumentQuery();

            return await GetResultsFromDocumentQuery(query);
        }

        public virtual IEnumerable<TEntity> GetAll()
        {
            return _client.CreateDocumentQuery<TEntity>
                (GetDocumentCollectionUri(), FeedOptions)
                .AsEnumerable();
        }

        public virtual Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(_client.CreateDocumentQuery<TEntity>
                (GetDocumentCollectionUri(), FeedOptions)
                .AsEnumerable());
        }

        public virtual TEntity GetById<TId>(TId id)
        {
            var document = _client.ReadDocumentAsync<TEntity>(GetDocumentUri(id.ToString()), RequestOptions).Result;
            return document.Document;
        }

        public virtual async Task<TEntity> GetByIdAsync<TId>(TId id)
        {
            var document = await _client.ReadDocumentAsync<TEntity>(GetDocumentUri(id.ToString()), RequestOptions);
            return document.Document;
        }

        public virtual void Update(TEntity entity)
        {
            _client.UpsertDocumentAsync(GetDocumentCollectionUri(), entity, RequestOptions).Wait();
        }

        public virtual async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default(CancellationToken))
        {
            await _client.UpsertDocumentAsync(GetDocumentCollectionUri(), entity, RequestOptions);
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

        public void Init<TOptions>(TOptions options)
        {
            if (options is RequestOptions requestOptions)
                RequestOptions = requestOptions;

            if (options is FeedOptions feedOptions)
                FeedOptions = feedOptions;
        }
    }
}