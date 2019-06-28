using System;
using Microsoft.Azure.Documents.Client;

namespace BoltOn.Data.CosmosDb
{
    public abstract class BaseCosmosDbContext
    {
        public string Uri { get; set; }
        public string AuthorizationKey { get; set; }
        public string DatabaseName { get; set; }
        public DocumentClient DocumentClient { get; set; }

        public void Configure(CosmosDbOptions configuration)
        {
            Uri = configuration.Uri;
            AuthorizationKey = configuration.AuthorizationKey;
            DatabaseName = configuration.DatabaseName;
            DocumentClient = new DocumentClient(new Uri(Uri), AuthorizationKey);
        }
    }
}
