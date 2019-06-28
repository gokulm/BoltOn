using System;
using Microsoft.Azure.Documents.Client;

namespace BoltOn.Data.CosmosDb
{
    public abstract class BaseCosmosDbContext<TCosmosDbContext> where TCosmosDbContext : BaseCosmosDbContext<TCosmosDbContext>
    {
        //public CosmosDbContextOptions Options { get; }
        public string Uri { get; set; }
        public string AuthorizationKey { get; set; }
        public string DatabaseName { get; set; }
        public DocumentClient DocumentClient { get; set; }

        protected BaseCosmosDbContext(CosmosDbContextOptions<TCosmosDbContext> options) 
        {
            //Options = options;
        }

        public void SetConfiguration(CosmosDbConfiguration configuration)
        {
            Uri = configuration.Uri;
            AuthorizationKey = configuration.AuthorizationKey;
            DatabaseName = configuration.DatabaseName;
            DocumentClient = new DocumentClient(new Uri(Uri), AuthorizationKey);
        }
    }
}
