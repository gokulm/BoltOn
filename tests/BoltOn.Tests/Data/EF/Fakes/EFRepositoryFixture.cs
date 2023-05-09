using System;
using BoltOn.Data;
using BoltOn.Data.EF;
using BoltOn.Tests.Other;
using Microsoft.Extensions.DependencyInjection;
using BoltOn.DataAbstractions.EF;

namespace BoltOn.Tests.Data.EF.Fakes
{
	public class EFQueryRepositoryFixture : IDisposable
	{
		public EFQueryRepositoryFixture()
		{
			IntegrationTestHelper.IsSqlServer = false;
			IntegrationTestHelper.IsSeedData = true;

			var serviceCollection = new ServiceCollection();
			serviceCollection
				.BoltOn(options =>
				{
					options.BoltOnEFModule();
					options.RegisterDataFakes();
				});
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
	}

	public class EFRepositoryFixture : IDisposable
	{
		public EFRepositoryFixture()
		{
			IntegrationTestHelper.IsSqlServer = false;
			IntegrationTestHelper.IsSeedData = true;

			var serviceCollection = new ServiceCollection();
			serviceCollection
				.BoltOn(options =>
				{
					options.BoltOnEFModule();
					options.RegisterDataFakes();
				});
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
