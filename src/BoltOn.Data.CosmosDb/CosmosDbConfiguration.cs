namespace BoltOn.Data.CosmosDb
{
    public class CosmosDbConfiguration
    {
        public string Uri { get; set; }
        public string AuthorizationKey { get; set; }
        public string DatabaseName { get; set; }
    }

    public class CosmosDbContextOptions
    {
        public string Uri { get; set; }
        public string AuthorizationKey { get; set; }
        public string DatabaseName { get; set; }

    }

    public class CosmosDbContextOptions<TCosmosDbContext> where TCosmosDbContext : BaseCosmosDbContext<TCosmosDbContext>
    {
        public CosmosDbContextOptions(CosmosDbContextOptions options)
        {
            Uri = options.Uri;
            AuthorizationKey = options.AuthorizationKey;
            DatabaseName = options.DatabaseName;
        }

        public string Uri { get; set; }
        public string AuthorizationKey { get; set; }
        public string DatabaseName { get; set; }
    }
}
