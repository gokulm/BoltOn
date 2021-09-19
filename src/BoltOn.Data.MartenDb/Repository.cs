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
        private readonly IDocumentSession _documentSession;

        public Repository(IDocumentSession documentSession)
        {
            _documentSession = documentSession;
        }

        public async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            _documentSession.Insert(entity);
            await _documentSession.SaveChangesAsync(cancellationToken);
            return entity;
        }

        public async Task<IEnumerable<TEntity>> AddAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        {
            _documentSession.Insert(entities);
            await _documentSession.SaveChangesAsync(cancellationToken);
            return entities;
        }

        public async Task DeleteAsync(object id, CancellationToken cancellationToken = default)
        {
            _documentSession.Delete(id);
            await _documentSession.SaveChangesAsync(cancellationToken);
        }

        public async Task<IEnumerable<TEntity>> FindByAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] includes)
        {
            return await _documentSession.Query<TEntity>().Where(predicate).ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _documentSession.Query<TEntity>().ToListAsync(cancellationToken);
        }

        public async Task<TEntity> GetByIdAsync(object id, CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] includes)
        {
            if(id.GetType() == typeof(string))
			{
                var tempId = id.ToString();
                return await _documentSession.LoadAsync<TEntity>(tempId, cancellationToken);
            }
            else if(id.GetType() == typeof(int))
            {
                var tempId = (int)Convert.ChangeType(id, typeof(int));
                return await _documentSession.LoadAsync<TEntity>(tempId, cancellationToken);
            }
            else if (id.GetType() == typeof(long))
            {
                var tempId = (long)Convert.ChangeType(id, typeof(long));
                return await _documentSession.LoadAsync<TEntity>(tempId, cancellationToken);
            }
            else if (id.GetType() == typeof(Guid))
            {
                var tempId = (Guid)Convert.ChangeType(id, typeof(Guid));
                return await _documentSession.LoadAsync<TEntity>(tempId, cancellationToken);
            }
            else
			{
                throw new Exception($"GetByIdAsync doesn't support fetching entity based on the Id of type {id.GetType()}");
			}
        }

        public async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            _documentSession.Update(entity);
            await _documentSession.SaveChangesAsync(cancellationToken);
        }
    }
}
