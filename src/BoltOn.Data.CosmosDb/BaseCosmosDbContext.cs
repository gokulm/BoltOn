using System;
using Microsoft.Azure.Documents.Client;

namespace BoltOn.Data.CosmosDb
{
    public abstract class BaseCosmosDbContext<TCosmosDbContext> where TCosmosDbContext : BaseCosmosDbContext<TCosmosDbContext>
    {
        public string Uri { get; set; }
        public string AuthorizationKey { get; set; }
        public string DatabaseName { get; set; }
        public DocumentClient DocumentClient { get; set; }

        protected BaseCosmosDbContext(CosmosDbContextOptions<TCosmosDbContext> options)
        {
            Uri = options.Uri;
            AuthorizationKey = options.AuthorizationKey;
            DatabaseName = options.DatabaseName;
            DocumentClient = new DocumentClient(new Uri(Uri), AuthorizationKey);
        }
    }
}
