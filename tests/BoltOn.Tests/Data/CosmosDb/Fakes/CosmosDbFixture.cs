using System;
using BoltOn.Data;
using BoltOn.Data.CosmosDb;
using BoltOn.Tests.Other;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.Tests.Data.CosmosDb.Fakes
{
    public class CosmosDbFixture : IDisposable
    {
        public CosmosDbFixture()
        {
			IntegrationTestHelper.IsCosmosDbServer = true;
			IntegrationTestHelper.IsSeedCosmosDbData = true;

			var serviceCollection = new ServiceCollection();
			serviceCollection
				.BoltOn(options =>
				{
					options.BoltOnCosmosDbModule();
					options.RegisterCosmosdbFakes();
				});
			ServiceProvider = serviceCollection.BuildServiceProvider();
			ServiceProvider.TightenBolts();
			SubjectUnderTest = ServiceProvider.GetService<IRepository<StudentFlattened>>();
		}

        public void Dispose()
        {
            ServiceProvider.LoosenBolts();
        }

        public IServiceProvider ServiceProvider { get; set; }

        public IRepository<StudentFlattened> SubjectUnderTest { get; set; }
    }
}
