using System;
using System.Threading.Tasks;
using BoltOn.Bootstrapping;
using BoltOn.Data.EF;
using BoltOn.Mediator.Pipeline;
using BoltOn.Tests.Common;
using BoltOn.Tests.Other;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BoltOn.Tests.Mediator
{
	[Collection("IntegrationTests")]
	public class MediatorSqlServerIntegrationTests : IDisposable
	{
		[Fact]
		[TestPriority(1)]
		public async Task Process_MediatorWithCommandRequestInSqlServer_AddsRecordInDbWithUoW()
		{
			if (!IntegrationTestHelper.IsSqlRunning)
				return;

			// arrange
			IntegrationTestHelper.IsSeedData = true;
			IntegrationTestHelper.IsSqlServer = true;
			var serviceCollection = new ServiceCollection();
			serviceCollection
				.BoltOn(options =>
				{
					options
						.BoltOnEFModule();
				})
				.AddLogging();
			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var sut = serviceProvider.GetService<IMediator>();

			// act
			var result = await sut.ProcessAsync(new AddStudentRequest { Id = 30, FirstName = "first", LastName = "last" });
			var dbContext = serviceProvider.GetService<IDbContextFactory>().Get<SchoolDbContext>();

			// assert 
			Assert.NotNull(result);
			var addedStudent = dbContext.Set<Student>().Find(30);
			Assert.NotNull(addedStudent);
		}

		[Fact]
		[TestPriority(2)]
		public async Task Process_MediatorWithQueryRequestInSqlServer_GetsRecord()
		{
			if (!IntegrationTestHelper.IsSqlRunning)
				return;

			// arrange
			IntegrationTestHelper.IsSqlServer = true;
			IntegrationTestHelper.IsSeedData = false;
			var serviceCollection = new ServiceCollection();
			serviceCollection
				.BoltOn(options =>
				{
					options
						.BoltOnEFModule();
				})
				.AddLogging();
			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var sut = serviceProvider.GetService<IMediator>();

			// act
			var result = await sut.ProcessAsync(new GetStudentRequest { StudentId = 1 });

			// assert 
			Assert.NotNull(result);
		}

		public void Dispose()
		{
			MediatorTestHelper.LoggerStatements.Clear();
			Bootstrapper
				.Instance
				.Dispose();
		}
	}
}
