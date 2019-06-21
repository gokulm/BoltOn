using System.Collections.Generic;

namespace BoltOn.Data.CosmosDb
{
    public class CosmosDbSettings
    {
        public Dictionary<string, CosmosDbConfiguration> CosmosDbs { get; set; }
    }
}
