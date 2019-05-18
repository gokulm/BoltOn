namespace BoltOn.Data.Cosmos
{
    public abstract class BaseCosmosContext
    {
        public Inner CosmosSetting;

        protected BaseCosmosContext(CosmosSettings settings, string databaseName)
        {
            CosmosSetting = settings.CosmosDbs[databaseName];
        }

        protected abstract void SetCosmosSetting();
    }
}
