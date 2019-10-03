using System;
using BoltOn.Cqrs;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.Data
{
	public interface ICqrsRepositoryFactory
	{
		IRepository<TEntity> GetRepository<TEntity>() where TEntity : BaseCqrsEntity;
	}

	public class CqrsRepositoryFactory : ICqrsRepositoryFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public CqrsRepositoryFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IRepository<TEntity> GetRepository<TEntity>() where TEntity : BaseCqrsEntity
        {
            var repository = _serviceProvider.GetService<IRepository<TEntity>>();
            return repository;
        }
    }
}
