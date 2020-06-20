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
			IntegrationTestHelper.IsCosmosDbServer = false;
			IntegrationTestHelper.IsSeedCosmosDbData = false;

			var serviceCollection = new ServiceCollection();
			serviceCollection
				.BoltOn(options =>
				{
					options.BoltOnCosmosDbModule();
					options.RegisterCosmosdbFakes();
				});

			if (IntegrationTestHelper.IsCosmosDbServer)
			{
				serviceCollection.AddTransient<IRepository<StudentFlattened>, Repository<StudentFlattened, TestSchoolCosmosDbOptions>>();
			}
			else
			{
				var repository = new Moq.Mock<IRepository<StudentFlattened>>();
				serviceCollection.AddTransient((s) => repository.Object);
			}

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
