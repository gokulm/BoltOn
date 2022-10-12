using System;
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
			IntegrationTestHelper.IsMartenDbRunning = false;

			if (!IntegrationTestHelper.IsMartenDbRunning)
				return;

			var serviceCollection = new ServiceCollection();
			serviceCollection.AddMarten(BuildStoreOptions());
			serviceCollection
				.BoltOn(options =>
				{
					options.BoltOnMartenDbModule(BuildStoreOptions());
				});

			serviceCollection.AddScoped<IRepository<Student>, Repository<Student>>();
			serviceCollection.AddScoped<IRepository<User>, Repository<User>>();

			ServiceProvider = serviceCollection.BuildServiceProvider();
			ServiceProvider.TightenBolts();
			SubjectUnderTest = ServiceProvider.GetService<IRepository<Student>>();
		}

		public void Dispose()
		{
			if (!IntegrationTestHelper.IsMartenDbRunning)
				return;

			ServiceProvider.LoosenBolts();
			IntegrationTestHelper.IsMartenDbRunning = false;
		}

		public IServiceProvider ServiceProvider { get; set; }

		public IRepository<Student> SubjectUnderTest { get; set; }

		private StoreOptions BuildStoreOptions()
		{
			var options = new StoreOptions();
			options.Connection("host=localhost;database=bolton;password=bolton;username=bolton");
			//options.AutoCreateSchemaObjects = Weasel.Postgresql.AutoCreate.All;

			return options;
		}
	}

	public class MartenDbQueryRepositoryFixture : IDisposable
	{
		public MartenDbQueryRepositoryFixture()
		{
			IntegrationTestHelper.IsMartenDbRunning = false;

			if (!IntegrationTestHelper.IsMartenDbRunning)
				return;

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
			if (!IntegrationTestHelper.IsMartenDbRunning)
				return;

			ServiceProvider.LoosenBolts();
			IntegrationTestHelper.IsMartenDbRunning = false;
		}

		public IServiceProvider ServiceProvider { get; set; }

		public IQueryRepository<Student> SubjectUnderTest { get; set; }

		private StoreOptions BuildStoreOptions()
		{
			var options = new StoreOptions();
			options.Connection("host=localhost;database=bolton;password=bolton;username=bolton");
			//options.AutoCreateSchemaObjects = Weasel.Postgresql.AutoCreate.All;

			return options;
		}
	}
}
