using System;
using BoltOn.Data;
using BoltOn.Data.Elasticsearch;
using BoltOn.Tests.Other;	
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.Tests.Data.Elasticsearch.Fakes
{
	public class ElasticDbFixture : IDisposable
    {
        public ElasticDbFixture()
        {
            IntegrationTestHelper.IsElasticsearchServer = true;
            IntegrationTestHelper.IsSeedElasticsearch = true;
            var serviceCollection = new ServiceCollection();
            serviceCollection
                .BoltOn(options =>
                {
                    options.BoltOnElasticsearchModule();
                    options.SetupFakes();
                });

            if (!IntegrationTestHelper.IsElasticsearchServer)
                return;

            serviceCollection.AddElasticsearch<TestElasticsearchOptions>(
                t => t.ConnectionSettings = new Nest.ConnectionSettings(new Uri("http://127.0.0.1:9200")));
            ServiceProvider = serviceCollection.BuildServiceProvider();
            ServiceProvider.TightenBolts();
        }

        public void Dispose()
        {
            ServiceProvider.LoosenBolts();
        }

        public IServiceProvider ServiceProvider { get; set; }
    }
}
