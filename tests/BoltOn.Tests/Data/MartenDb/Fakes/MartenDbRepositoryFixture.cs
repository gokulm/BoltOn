using System;
using BoltOn.Data;
using BoltOn.Data.MartenDb;
using BoltOn.Tests.Other;
using Marten;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.Tests.Data.MartenDb.Fakes
{
	public class MartenDbRepositoryFixture : IDisposable
	{
		public MartenDbRepositoryFixture()
		{
			IntegrationTestHelper.IsMartenDbRunning = true;
			IntegrationTestHelper.IsSeedMartenDb = true;

			var serviceCollection = new ServiceCollection();
			serviceCollection.AddMarten(BuildStoreOptions());
			serviceCollection
				.BoltOn(options =>
				{
					options.BoltOnMartenDbModule(BuildStoreOptions());
				});

			serviceCollection.AddTransient<IRepository<Student>, Repository<Student>>();

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

		private StoreOptions BuildStoreOptions()
		{
			var options = new StoreOptions();
			options.Connection("host=localhost;database=bolton;password=bolton;username=bolton");
			options.AutoCreateSchemaObjects = AutoCreate.All;

			return options;
		}
	}

	public class MartenDbQueryRepositoryFixture : IDisposable
	{
		public MartenDbQueryRepositoryFixture()
		{
			IntegrationTestHelper.IsMartenDbRunning = true;
			IntegrationTestHelper.IsSeedMartenDb = true;

			var serviceCollection = new ServiceCollection();
			serviceCollection.AddMarten(BuildStoreOptions());
			serviceCollection
				.BoltOn(options =>
				{
					options.BoltOnMartenDbModule(BuildStoreOptions());
				});

			serviceCollection.AddTransient<IQueryRepository<Student>, QueryRepository<Student>>();

			ServiceProvider = serviceCollection.BuildServiceProvider();
			ServiceProvider.TightenBolts();
			SubjectUnderTest = ServiceProvider.GetService<IQueryRepository<Student>>();
		}

		public void Dispose()
		{
			ServiceProvider.LoosenBolts();
		}

		public IServiceProvider ServiceProvider { get; set; }

		public IQueryRepository<Student> SubjectUnderTest { get; set; }

		private StoreOptions BuildStoreOptions()
		{
			var options = new StoreOptions();
			options.Connection("host=localhost;database=bolton;password=bolton;username=bolton");
			options.AutoCreateSchemaObjects = AutoCreate.All;

			return options;
		}
	}
}
