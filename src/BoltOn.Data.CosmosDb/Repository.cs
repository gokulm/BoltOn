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

        public virtual async Task<TEntity> AddAsync(TEntity entity, object options = null, CancellationToken cancellationToken = default)
        {
            PublishEvents(entity);
            if (options is RequestOptions requestOptions)
                await DocumentClient.CreateDocumentAsync(DocumentCollectionUri, entity, requestOptions, cancellationToken: cancellationToken);
            else
                await DocumentClient.CreateDocumentAsync(DocumentCollectionUri, entity, cancellationToken: cancellationToken);
            return entity;
        }

        public virtual async Task<IEnumerable<TEntity>> FindByAsync(Expression<Func<TEntity, bool>> predicate,
            object options = null,
            CancellationToken cancellationToken = default)
        {
            IOrderedQueryable<TEntity> orderedQueryable;
            if (options is FeedOptions feedOptions)
                orderedQueryable = DocumentClient.CreateDocumentQuery<TEntity>(DocumentCollectionUri, feedOptions);
            else
                orderedQueryable = DocumentClient.CreateDocumentQuery<TEntity>(DocumentCollectionUri);

            var query = orderedQueryable
               .Where(predicate)
               .AsDocumentQuery();

            return await GetResultsFromDocumentQuery(query, cancellationToken);
        }

        public virtual async Task<IEnumerable<TEntity>> GetAllAsync(object options = null, CancellationToken cancellationToken = default)
        {
            IOrderedQueryable<TEntity> orderedQueryable;
            if (options is FeedOptions feedOptions)
                orderedQueryable = DocumentClient.CreateDocumentQuery<TEntity>(DocumentCollectionUri, feedOptions);
            else
                orderedQueryable = DocumentClient.CreateDocumentQuery<TEntity>(DocumentCollectionUri);

            return await GetResultsFromDocumentQuery(orderedQueryable.AsDocumentQuery(), cancellationToken);
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

        public virtual async Task UpdateAsync(TEntity entity, object options = null, CancellationToken cancellationToken = default)
        {
            PublishEvents(entity);
            if (options is RequestOptions requestOptions)
                await DocumentClient.UpsertDocumentAsync(DocumentCollectionUri, entity, requestOptions, cancellationToken: cancellationToken);
            else
                await DocumentClient.UpsertDocumentAsync(DocumentCollectionUri, entity, cancellationToken: cancellationToken);
		}

		public virtual async Task DeleteAsync(TEntity entity, object options = null, CancellationToken cancellationToken = default)
		{
			PublishEvents(entity);
			dynamic entityWithId = entity;
			if (options is RequestOptions requestOptions)
				await DocumentClient.DeleteDocumentAsync(GetDocumentUri(entityWithId.Id.ToString()), requestOptions);
			else
				await DocumentClient.DeleteDocumentAsync(GetDocumentUri(entityWithId.Id.ToString()));
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

        protected void PublishEvents(TEntity entity)
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
