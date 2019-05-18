using System.Collections.Generic;

namespace BoltOn.Data.Cosmos
{
    public class CosmosSettings
    {
        public Dictionary<string, Inner> CosmosDbs { get; set; }
    }

    public class Inner
    {
        public string Uri { get; set; }
        public string AuthorizationKey { get; set; }
        public string DatabaseName { get; set; }
    }
}
