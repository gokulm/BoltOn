namespace BoltOn.Data.CosmosDb
{
    public abstract class BaseCosmosDbOptions
    {
        public string Uri { get; set; }
        public string AuthorizationKey { get; set; }
        public string DatabaseName { get; set; }
    }
}
