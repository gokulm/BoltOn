using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Marten;
using System.Linq;

namespace BoltOn.Data.MartenDb
{
	public class QueryRepository<TEntity> : IQueryRepository<TEntity>
		where TEntity : class
	{
		private readonly IQuerySession _querySession;

		public QueryRepository(IQuerySession querySession)
		{
			_querySession = querySession;
		}

		public async Task<IEnumerable<TEntity>> FindByAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] includes)
		{
			return await _querySession.Query<TEntity>().Where(predicate).ToListAsync(cancellationToken);
		}

		public async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
		{
			return await _querySession.Query<TEntity>().ToListAsync(cancellationToken);
		}

		public async Task<TEntity> GetByIdAsync(object id, CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] includes)
		{
			return await _querySession.LoadAsync<TEntity>(id.ToString(), cancellationToken);
		}
	}
}
