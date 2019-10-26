using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using BoltOn.Cqrs;
using BoltOn.Utilities;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Nito.AsyncEx;

namespace BoltOn.Data.CosmosDb
{
    public class Repository<TEntity, TCosmosDbOptions> : IRepository<TEntity>
        where TEntity : class
        where TCosmosDbOptions : BaseCosmosDbOptions
    {
		private readonly EventBag _eventBag;
		private readonly IBoltOnClock _boltOnClock;

		protected string DatabaseName { get; private set; }
		protected string CollectionName { get; private set; }
		protected DocumentClient DocumentClient { get; private set; }
		protected Uri DocumentCollectionUri { get; private set; }

		public Repository(TCosmosDbOptions options, EventBag eventBag,
			IBoltOnClock boltOnClock, string collectionName = null)
        {
            DatabaseName = options.DatabaseName;
			_eventBag = eventBag;
			_boltOnClock = boltOnClock;
			CollectionName = collectionName ?? typeof(TEntity).Name.Pluralize();
            DocumentClient = new DocumentClient(new Uri(options.Uri), options.AuthorizationKey);
			DocumentCollectionUri = UriFactory.CreateDocumentCollectionUri(DatabaseName, CollectionName);
		}

        public virtual TEntity Add(TEntity entity, object options = null)
        {
            AsyncContext.Run(() => DocumentClient.CreateDocumentAsync(DocumentCollectionUri, entity));
            return entity;
        }

        public virtual async Task<TEntity> AddAsync(TEntity entity, object options = null, CancellationToken cancellationToken = default)
        {
			PublishEvents(entity);
            await DocumentClient.CreateDocumentAsync(DocumentCollectionUri, entity, cancellationToken: cancellationToken);
            return entity;
        }

        public virtual IEnumerable<TEntity> FindBy(Expression<Func<TEntity, bool>> predicate, object options = null,
			params Expression<Func<TEntity, object>>[] includes)
        {
            var query = DocumentClient.CreateDocumentQuery<TEntity>(DocumentCollectionUri)
                .Where(predicate)
                .AsDocumentQuery();

            return AsyncContext.Run(() => GetResultsFromDocumentQuery(query));
        }

        public virtual async Task<IEnumerable<TEntity>> FindByAsync(Expression<Func<TEntity, bool>> predicate,
			object options = null,
			CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] includes)
        {
            var query = DocumentClient.CreateDocumentQuery<TEntity>(DocumentCollectionUri)
               .Where(predicate)
               .AsDocumentQuery();

            return await GetResultsFromDocumentQuery(query);
        }

        public virtual IEnumerable<TEntity> GetAll(object options = null)
        {
            var query = DocumentClient.CreateDocumentQuery<TEntity>
                (DocumentCollectionUri)
                .AsDocumentQuery();

            return AsyncContext.Run(() => GetResultsFromDocumentQuery(query));
        }

        public virtual async Task<IEnumerable<TEntity>> GetAllAsync(object options = null, CancellationToken cancellationToken = default)
        {
            var query = DocumentClient.CreateDocumentQuery<TEntity>
                (DocumentCollectionUri)
                .AsDocumentQuery();

            return await GetResultsFromDocumentQuery(query);
        }

        public virtual TEntity GetById(object id, object options = null)
        {
            var document = AsyncContext.Run(() => DocumentClient.ReadDocumentAsync<TEntity>(GetDocumentUri(id.ToString())));
            return document.Document;
        }

        public virtual async Task<TEntity> GetByIdAsync(object id, object options = null, CancellationToken cancellationToken = default)
        {
			try
			{
				if (options is RequestOptions requestOptions)
				{
					return await DocumentClient.ReadDocumentAsync<TEntity>(GetDocumentUri(id.ToString()),
						requestOptions, cancellationToken);
				}
				return await DocumentClient.ReadDocumentAsync<TEntity>(GetDocumentUri(id.ToString()), cancellationToken: cancellationToken);
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

        public virtual void Update(TEntity entity, object options = null)
        {
            AsyncContext.Run(() => DocumentClient.UpsertDocumentAsync(DocumentCollectionUri, entity));
        }

        public virtual async Task UpdateAsync(TEntity entity, object options = null, CancellationToken cancellationToken = default)
		{
			PublishEvents(entity);
			await DocumentClient.UpsertDocumentAsync(DocumentCollectionUri, entity, cancellationToken: cancellationToken);
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

		private void PublishEvents(TEntity entity)
		{
			if (entity is BaseCqrsEntity baseCqrsEntity)
			{
				var eventsToBeProcessed = baseCqrsEntity.EventsToBeProcessed.ToList()
					.Where(w => !w.CreatedDate.HasValue);
				foreach (var @event in eventsToBeProcessed)
				{
					@event.CreatedDate = _boltOnClock.Now;
					_eventBag.EventsToBeProcessed.Add(@event);
				}

				var processedEvents = baseCqrsEntity.ProcessedEvents.ToList()
					.Where(w => !w.ProcessedDate.HasValue);
				foreach (var @event in processedEvents)
				{
					@event.ProcessedDate = _boltOnClock.Now;
				}
			}
		}
	}
}
