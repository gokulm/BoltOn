using Microsoft.Extensions.Options;

namespace BoltOn.Data.CosmosDb
{
    public abstract class BaseCosmosDbContext
    {
        public CosmosDbConfiguration CosmosDbConfiguration;

        protected BaseCosmosDbContext(IOptions<CosmosDbSettings> settings, string databaseName)
        {
            CosmosDbConfiguration = settings.Value.CosmosDbs[databaseName];
        }

        protected virtual void SetCosmosDbSetting() { }
    }
}
