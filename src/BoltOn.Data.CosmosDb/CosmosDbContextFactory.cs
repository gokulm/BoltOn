using System;

namespace BoltOn.Data.CosmosDb
{
    public interface ICosmosDbContextFactory
    {
        TCosmosDbContext Get<TCosmosDbContext>() where TCosmosDbContext : BaseCosmosDbContext;
    }

    public class CosmosDbContextFactory : ICosmosDbContextFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public CosmosDbContextFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public TCosmosDbContext Get<TCosmosDbContext>() where TCosmosDbContext : BaseCosmosDbContext
        {
            if (!(_serviceProvider.GetService(typeof(TCosmosDbContext)) is TCosmosDbContext CosmosDbContext))
                throw new Exception("CosmosDbContext is null");

            return CosmosDbContext;
        }
    }
}
