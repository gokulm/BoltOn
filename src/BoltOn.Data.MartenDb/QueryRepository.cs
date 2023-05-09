using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Marten;
using System.Linq;
using BoltOn.DataAbstractions.MartenDb;

namespace BoltOn.Data.MartenDb
{
	public class QueryRepository<TEntity> : IQueryRepository<TEntity>
		where TEntity : class
	{
		private readonly IDocumentStore _documentStore;

		public QueryRepository(IDocumentStore documentStore)
		{
			_documentStore = documentStore;
		}

		public async Task<IEnumerable<TEntity>> FindByAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
		{
			using var querySession = _documentStore.QuerySession();
			return await querySession.Query<TEntity>().Where(predicate).ToListAsync(cancellationToken);
		}

		public async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
		{
			using var querySession = _documentStore.QuerySession();
			return await querySession.Query<TEntity>().ToListAsync(cancellationToken);
		}

		public async Task<TEntity> GetByIdAsync(object id, CancellationToken cancellationToken = default)
		{
			using var querySession = _documentStore.QuerySession();

			if (id.GetType() == typeof(string))
			{
				var tempId = id.ToString();
				return await querySession.LoadAsync<TEntity>(tempId, cancellationToken);
			}
			else if (id.GetType() == typeof(int))
			{
				var tempId = (int)Convert.ChangeType(id, typeof(int));
				return await querySession.LoadAsync<TEntity>(tempId, cancellationToken);
			}
			else if (id.GetType() == typeof(long))
			{
				var tempId = (long)Convert.ChangeType(id, typeof(long));
				return await querySession.LoadAsync<TEntity>(tempId, cancellationToken);
			}
			else if (id.GetType() == typeof(Guid))
			{
				var tempId = (Guid)Convert.ChangeType(id, typeof(Guid));
				return await querySession.LoadAsync<TEntity>(tempId, cancellationToken);
			}
			else
			{
				throw new Exception($"GetByIdAsync doesn't support fetching entity based on the Id of type {id.GetType()}");
			}
		}
	}
}
