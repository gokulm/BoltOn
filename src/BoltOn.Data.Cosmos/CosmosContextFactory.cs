using System;

namespace BoltOn.Data.Cosmos
{
    public interface ICosmosContextFactory
    {
        TCosmosContext Get<TCosmosContext>() where TCosmosContext : BaseCosmosContext;
    }

    public class CosmosContextFactory : ICosmosContextFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public CosmosContextFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public TCosmosContext Get<TCosmosContext>() where TCosmosContext : BaseCosmosContext
        {
            if (!(_serviceProvider.GetService(typeof(TCosmosContext)) is TCosmosContext cosmosContext))
                throw new Exception("CosmosContext is null");

            return cosmosContext;
        }
    }
}
