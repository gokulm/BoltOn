using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Marten;
using System.Linq;

namespace BoltOn.Data.MartenDb
{
	public class Repository<TEntity> : IRepository<TEntity>
		where TEntity : class
	{
		private readonly IDocumentStore _documentStore;

		public Repository(IDocumentStore documentStore)
		{
			_documentStore = documentStore;
		}

		public async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
		{
			using var session = _documentStore.OpenSession();
			session.Insert(entity);
			await session.SaveChangesAsync(cancellationToken);
			return entity;
		}

		public async Task<IEnumerable<TEntity>> AddAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
		{
			using var session = _documentStore.OpenSession();
			session.Insert(entities);
			await session.SaveChangesAsync(cancellationToken);
			return entities;
		}

		public async Task DeleteAsync(object id, CancellationToken cancellationToken = default)
		{
			using var session = _documentStore.OpenSession();
			session.Delete(await GetByIdAsync(id, cancellationToken));
			await session.SaveChangesAsync(cancellationToken);
		}

		public async Task<IEnumerable<TEntity>> FindByAsync(Expression<Func<TEntity, bool>> predicate,
			CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] includes)
		{
			if (includes != null)
				throw new Exception("includes is not supported");

			using var session = _documentStore.OpenSession();
			return await session.Query<TEntity>().Where(predicate).ToListAsync(cancellationToken);
		}

		public async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
		{
			using var session = _documentStore.OpenSession();
			return await session.Query<TEntity>().ToListAsync(cancellationToken);
		}

		public async Task<TEntity> GetByIdAsync(object id, CancellationToken cancellationToken = default,
			params Expression<Func<TEntity, object>>[] includes)
		{
			if (includes != null)
				throw new Exception("includes is not supported");

			using var session = _documentStore.OpenSession();
			if (id.GetType() == typeof(string))
			{
				var tempId = id.ToString();
				return await session.LoadAsync<TEntity>(tempId, cancellationToken);
			}
			else if (id.GetType() == typeof(int))
			{
				var tempId = (int)Convert.ChangeType(id, typeof(int));
				return await session.LoadAsync<TEntity>(tempId, cancellationToken);
			}
			else if (id.GetType() == typeof(long))
			{
				var tempId = (long)Convert.ChangeType(id, typeof(long));
				return await session.LoadAsync<TEntity>(tempId, cancellationToken);
			}
			else if (id.GetType() == typeof(Guid))
			{
				var tempId = (Guid)Convert.ChangeType(id, typeof(Guid));
				return await session.LoadAsync<TEntity>(tempId, cancellationToken);
			}
			else
			{
				throw new Exception($"GetByIdAsync doesn't support fetching entity based on the Id of type {id.GetType()}");
			}
		}

		public async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
		{
			using var session = _documentStore.OpenSession();
			session.Update(entity);
			await session.SaveChangesAsync(cancellationToken);
		}
	}
}
