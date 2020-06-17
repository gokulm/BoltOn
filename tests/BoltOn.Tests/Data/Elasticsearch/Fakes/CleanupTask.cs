using System;
using BoltOn.Bootstrapping;
using BoltOn.Tests.Other;
using Nest;

namespace BoltOn.Tests.Data.Elasticsearch.Fakes
{
    public class CleanupTask : ICleanupTask
    {
        public void Run()
        {
            if (IntegrationTestHelper.IsElasticsearchServer && IntegrationTestHelper.IsSeedElasticsearch)
            {
                var settings = new ConnectionSettings(new Uri("http://127.0.0.1:9200"))
                       .DefaultIndex("students");

                var client = new ElasticClient(settings);
                client.Indices.Delete("students");
            }
        }
    }
}
