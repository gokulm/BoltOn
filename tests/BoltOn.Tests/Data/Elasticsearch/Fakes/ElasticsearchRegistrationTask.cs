using System;
using BoltOn.Bootstrapping;
using BoltOn.Data;
using BoltOn.Data.Elasticsearch;
using BoltOn.Tests.Other;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.Tests.Data.Elasticsearch.Fakes
{
	public static class ElasticsearchRegistrationTask
	{
		public static void RegisterElasticsearchFakes(this BoltOnOptions boltOnOptions)
		{
			if (IntegrationTestHelper.IsElasticsearchServer)
			{
				var uri = new Uri("http://localhost:9200/");
				boltOnOptions.ServiceCollection.AddElasticsearch<TestElasticsearchOptions>(e =>
				{
					e.ConnectionSettings = new Nest.ConnectionSettings(uri);
				});

				boltOnOptions.ServiceCollection.AddTransient<IRepository<Employee>, Repository<Employee, TestElasticsearchOptions>>();
			}
		}
	}

	public class TestElasticsearchOptions : BaseElasticsearchOptions
	{
	}
}
