using System;
using System.Threading.Tasks;
using BoltOn.Data.EF;
using BoltOn.Requestor.Pipeline;
using BoltOn.Tests.Common;
using BoltOn.Tests.Other;
using BoltOn.Tests.Requestor.Fakes;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BoltOn.Tests.Requestor
{
	[Collection("IntegrationTests")]
	public class RequestorSqlServerIntegrationTests : IDisposable
	{
		[Fact]
		[TestPriority(1)]
		public async Task Process_RequestorWithCommandRequestInSqlServer_AddsRecordInDbWithinTransaction()
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
			var sut = serviceProvider.GetService<IRequestor>();

			// act
			var result = await sut.ProcessAsync(new AddStudentRequest { Id = 30, FirstName = "first", LastName = "last" });
			var dbContext = serviceProvider.GetService<SchoolDbContext>();

			// assert 
			Assert.NotNull(result);
			var addedStudent = dbContext.Set<Student>().Find(30);
			Assert.NotNull(addedStudent);
		}

		[Fact]
		[TestPriority(2)]
		public async Task Process_RequestorWithQueryRequestInSqlServer_GetsRecord()
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
			var sut = serviceProvider.GetService<IRequestor>();

			// act
			var result = await sut.ProcessAsync(new GetStudentRequest { StudentId = 1 });

			// assert 
			Assert.NotNull(result);
		}

		public void Dispose()
		{
			RequestorTestHelper.LoggerStatements.Clear();
		}
	}
}
