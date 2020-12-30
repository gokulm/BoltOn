using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nest;

namespace BoltOn.Data.Elasticsearch
{
	public class Repository<TEntity, TElasticsearchOptions> : IRepository<TEntity>
		where TEntity : class
		where TElasticsearchOptions : BaseElasticsearchOptions
	{
		protected virtual string IndexName
		{
			get
			{
				return typeof(TEntity).Name.Pluralize().ToLower();
			}
		}

		protected ElasticClient Client { get; }

		public Repository(TElasticsearchOptions elasticsearchOptions)
		{
			Client = new ElasticClient(elasticsearchOptions.ConnectionSettings);
		}

		public virtual async Task<TEntity> GetByIdAsync(object id, CancellationToken cancellationToken = default)
		{
			var result = await Client.GetAsync<TEntity>(id.ToString(), idx => idx.Index(IndexName),
				cancellationToken);
			if (result.OriginalException != null)
				throw result.OriginalException;
			return result.Source;
		}

		public virtual async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
		{
			var result = await Client.SearchAsync<TEntity>(search =>
				search.MatchAll().Index(IndexName), cancellationToken);
			if (result.OriginalException != null)
				throw result.OriginalException;
			return result.Documents;
		}

		public virtual async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
		{
			var result = await AddAsync(new List<TEntity> { entity }, cancellationToken);
			return result.FirstOrDefault();
		}

		public virtual async Task<IEnumerable<TEntity>> AddAsync(IEnumerable<TEntity> entities,
			CancellationToken cancellationToken = default)
		{
			var tempEntities = entities.ToList();
			foreach (var entity in tempEntities)
			{
				var result = await Client.IndexAsync(entity, idx => idx.Index(IndexName), cancellationToken);
				if (!result.IsValid)
					throw result.OriginalException;
			}
			return tempEntities;
		}

		public virtual async Task UpdateAsync(TEntity entity,
			CancellationToken cancellationToken = default)
		{
			var result = await Client.UpdateAsync(new DocumentPath<TEntity>(entity),
												u => u.Doc(entity).Index(IndexName), cancellationToken);
			if (!result.IsValid)
				throw result.OriginalException;
		}

		public virtual async Task DeleteAsync(object id,
			CancellationToken cancellationToken = default)
		{
			var result = await Client.DeleteAsync<TEntity>(id.ToString(),
															idx => idx.Index(IndexName), cancellationToken);
			if (!result.IsValid)
				throw result.OriginalException;
		}

		public virtual async Task<IEnumerable<TEntity>> FindByAsync(SearchRequest searchRequest = null,
			CancellationToken cancellationToken = default)
		{
			if (searchRequest != null)
			{
				Func<QueryContainerDescriptor<TEntity>, QueryContainer> func = (q) => searchRequest.Query;
				var result = await Client.SearchAsync<TEntity>(s => s.Index(IndexName).Query(func), cancellationToken);
				if (!result.IsValid)
					throw result.OriginalException;
				return result.Documents;
			}
			return null;
		}
	}
}
