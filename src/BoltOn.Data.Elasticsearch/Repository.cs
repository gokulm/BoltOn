using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Nest;

namespace BoltOn.Data.Elasticsearch
{
	public class Repository<TEntity, TElasticsearchOptions> : IRepository<TEntity>
		where TEntity : class
		where TElasticsearchOptions : BaseElasticsearchOptions
	{
		protected string IndexName { get; set; }

		public ElasticClient Client { get; }

		public Repository(TElasticsearchOptions elasticsearchOptions)
		{
			Client = new ElasticClient(elasticsearchOptions.ConnectionSettings);
			InitializeIndexName();
		}

		public virtual void InitializeIndexName()
		{
			IndexName = typeof(TEntity).Name.Pluralize().ToLower();
		}

		public virtual async Task<TEntity> AddAsync(TEntity entity, object options = null,
			CancellationToken cancellationToken = default)
		{
			var result = await AddAsync(new List<TEntity> { entity }, options, cancellationToken);
			return result.FirstOrDefault();
		}

		public virtual async Task<IEnumerable<TEntity>> AddAsync(IEnumerable<TEntity> entities, object options = null,
			CancellationToken cancellationToken = default)
		{
			var tempEntities = entities.ToList();
			foreach (var entity in tempEntities)
			{
				var result = await Client.IndexAsync(entity, idx => idx.Index(IndexName), cancellationToken);
				if (!result.IsValid)
				{
					throw result.OriginalException;
				}
			}
			return tempEntities;
		}

		public virtual async Task<TEntity> GetByIdAsync(object id, object options = null,
			CancellationToken cancellationToken = default)
		{
			var result = await Client.GetAsync<TEntity>(id.ToString(), idx => idx.Index(IndexName),
				cancellationToken);
			//if (!result.IsValid)
			//{
			//    throw result.OriginalException;
			//}
			return result.Source;
		}

		public virtual async Task<IEnumerable<TEntity>> GetAllAsync(object options = null,
			CancellationToken cancellationToken = default)
		{
			if (options != null && options is SearchRequest searchRequest)
			{
				var result = await Client.SearchAsync<TEntity>(searchRequest, cancellationToken);
				return result.Documents;
			}
			else
			{
				var result = await Client.SearchAsync<TEntity>(search =>
					search.MatchAll().Index(IndexName), cancellationToken);
				return result.Documents;
			}
		}

		public virtual async Task UpdateAsync(TEntity entity, object options = null,
			CancellationToken cancellationToken = default)
		{
			var result = await Client.UpdateAsync(new DocumentPath<TEntity>(entity),
												u => u.Doc(entity).Index(IndexName), cancellationToken);
			if (!result.IsValid)
			{
				throw result.OriginalException;
			}
		}

		public virtual async Task DeleteAsync(object id, object options = null,
			CancellationToken cancellationToken = default)
		{
			var result = await Client.DeleteAsync<TEntity>(id.ToString(),
															idx => idx.Index(IndexName), cancellationToken);
			if (!result.IsValid)
			{
				throw result.OriginalException;
			}
		}

		/// <summary>
		/// Elasticsearch NEST library does not support search by predicate, so pass NEST's SearchRequest
		/// object for options parameter and null for predicate parameter
		/// </summary>
		public virtual async Task<IEnumerable<TEntity>> FindByAsync(Expression<Func<TEntity, bool>> predicate,
			object options = null, CancellationToken cancellationToken = default)
		{
			if (predicate != null)
				throw new NotImplementedException("Bolton Elasticsearch does not support search by predicate. " +
					"Pass null to the predicate param and NEST'S SearchRequest object for the options param");

			if (options != null && options is SearchRequest searchRequest)
			{
				var result = await Client.SearchAsync<TEntity>(searchRequest, cancellationToken);
				if (!result.IsValid)
				{
					throw result.OriginalException;
				}

				return result.Documents;
			}
			return null;
		}
	}
}
