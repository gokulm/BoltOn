using System.Collections.Generic;

namespace BoltOn.Data.CosmosDb
{
    public class CosmosDbSettings
    {
        public Dictionary<string, Inner> CosmosDbDbs { get; set; }
    }

    public class Inner
    {
        public string Uri { get; set; }
        public string AuthorizationKey { get; set; }
        public string DatabaseName { get; set; }
    }
}
