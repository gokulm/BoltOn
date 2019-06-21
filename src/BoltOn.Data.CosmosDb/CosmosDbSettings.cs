using System.Collections.Generic;

namespace BoltOn.Data.CosmosDb
{
    public class CosmosDbSettings
    {
        public Dictionary<string, CosmosDbDetails> CosmosDbs { get; set; }
    }

    public class CosmosDbDetails
    {
        public string Uri { get; set; }
        public string AuthorizationKey { get; set; }
        public string DatabaseName { get; set; }
    }
}
