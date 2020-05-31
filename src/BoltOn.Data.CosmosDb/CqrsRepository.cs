using System;
using System.Linq;
using BoltOn.Bootstrapping;
using BoltOn.Cqrs;

namespace BoltOn.Data.CosmosDb
{
	public class CqrsRepository<TEntity, TCosmosDbOptions> : Repository<TEntity, TCosmosDbOptions>
        where TEntity : BaseCqrsEntity
        where TCosmosDbOptions : BaseCosmosDbOptions
    {
        private readonly EventBag _eventBag;
		private readonly CqrsOptions _cqrsOptions;

		public CqrsRepository(TCosmosDbOptions options, EventBag eventBag,
            CqrsOptions cqrsOptions, string collectionName = null) : base(options, collectionName)
        {
			_eventBag = eventBag;
			_cqrsOptions = cqrsOptions;
		}

		protected override void PublishEvents(TEntity entity)
		{
			entity.EventsToBeProcessed.ToList().ForEach(e => _eventBag.EventsToBeProcessed.Add(e));

			if (entity.ProcessedEvents.Any() && _cqrsOptions.PurgeEventsProcessedBefore.HasValue)
			{
				var timeStamp = DateTime.UtcNow.Add(-1 * _cqrsOptions.PurgeEventsProcessedBefore.Value);
				var processedEventsToBeRemoved = entity.ProcessedEvents.Where(w => w.ProcessedDate < timeStamp).ToList();
				processedEventsToBeRemoved.ForEach(e => entity.RemoveProcessedEvent(e));
			}
		}
	}
}