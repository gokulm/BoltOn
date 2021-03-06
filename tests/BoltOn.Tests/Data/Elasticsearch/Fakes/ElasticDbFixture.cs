﻿using System;
using BoltOn.Data.Elasticsearch;
using BoltOn.Tests.Other;	
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.Tests.Data.Elasticsearch.Fakes
{
	public class ElasticDbFixture : IDisposable
    {
        public ElasticDbFixture()
        {
            IntegrationTestHelper.IsElasticsearchServer = false;
            IntegrationTestHelper.IsSeedElasticsearch = false;
            var serviceCollection = new ServiceCollection();
            serviceCollection
                .BoltOn(options =>
                {
                    options.BoltOnElasticsearchModule();
                    options.SetupFakes();
                });

            serviceCollection.AddElasticsearch<TestElasticsearchOptions>(
                t => t.ConnectionSettings = new Nest.ConnectionSettings(new Uri("http://127.0.0.1:9200")));
            ServiceProvider = serviceCollection.BuildServiceProvider();
            ServiceProvider.TightenBolts();
            SubjectUnderTest = ServiceProvider.GetService<IRepository<Student>>();
        }

        public void Dispose()
        {
			ServiceProvider.LoosenBolts();
		}

        public IServiceProvider ServiceProvider { get; set; }

        public IRepository<Student> SubjectUnderTest { get; set; }
    }
}
