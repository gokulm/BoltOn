using System;
using Microsoft.Azure.Documents.Client;

namespace BoltOn.Data.CosmosDb
{
    public abstract class BaseCosmosDbContext<TCosmosDbContext> where TCosmosDbContext : BaseCosmosDbContext<TCosmosDbContext>
    {
        public string Uri { get; private set; }
        public string AuthorizationKey { get; private set; }
        public string DatabaseName { get; private set; }
        public DocumentClient DocumentClient { get; private set; }

        protected BaseCosmosDbContext(CosmosDbContextOptions<TCosmosDbContext> options)
        {
            Uri = options.Uri;
            AuthorizationKey = options.AuthorizationKey;
            DatabaseName = options.DatabaseName;
            DocumentClient = new DocumentClient(new Uri(Uri), AuthorizationKey);
        }
    }
}
