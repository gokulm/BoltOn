using Microsoft.Extensions.Options;

namespace BoltOn.Data.CosmosDb
{
    public abstract class BaseCosmosDbContext
    {
        public CosmosDbConfiguration CosmosDbSetting;

        protected BaseCosmosDbContext(IOptions<CosmosDbSettings> settings, string databaseName)
        {
            CosmosDbSetting = settings.Value.CosmosDbs[databaseName];
        }

        protected virtual void SetCosmosDbSetting() { }
    }
}
