namespace BoltOn.Data.CosmosDb
{
    public abstract class BaseCosmosDbContext
    {
        public Inner CosmosDbSetting;

        protected BaseCosmosDbContext(CosmosDbSettings settings, string databaseName)
        {
            CosmosDbSetting = settings.CosmosDbDbs[databaseName];
        }

        protected abstract void SetCosmosDbSetting();
    }
}
