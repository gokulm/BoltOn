using System;
using BoltOn.Bootstrapping;
using BoltOn.Data.Elasticsearch;
using BoltOn.Tests.Other;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.Tests.Data.Elasticsearch.Fakes
{
	public static class Extensions
	{
		public static void SetupFakes(this BootstrapperOptions bootstrapperOptions)
		{
			if (IntegrationTestHelper.IsElasticsearchServer)
			{
				var uri = new Uri("http://localhost:9200/");
				bootstrapperOptions.ServiceCollection.AddElasticsearch<TestElasticsearchOptions>(e =>
				{
					e.ConnectionSettings = new Nest.ConnectionSettings(uri);
				});

				bootstrapperOptions.ServiceCollection.AddTransient<IRepository<Student>, Repository<Student, TestElasticsearchOptions>>();
			}
		}
	}

	public class TestElasticsearchOptions : BaseElasticsearchOptions
	{
	}
}
