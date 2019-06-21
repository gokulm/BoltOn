namespace BoltOn.Data.CosmosDb
{
    public abstract class BaseCosmosDbContext
    {
        public CosmosDbDetails CosmosDbSetting;

        protected BaseCosmosDbContext(CosmosDbSettings settings, string databaseName)
        {
            CosmosDbSetting = settings.CosmosDbs[databaseName];
        }

        protected abstract void SetCosmosDbSetting();
    }
}
