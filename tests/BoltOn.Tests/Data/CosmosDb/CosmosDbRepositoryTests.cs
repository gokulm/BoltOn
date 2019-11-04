using System;
using System.Threading.Tasks;
using BoltOn.Data;
using BoltOn.Data.CosmosDb;
using BoltOn.Tests.Other;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BoltOn.Tests.Data.CosmosDb
{
	[Collection("IntegrationTests")]
	public class CosmosDbRepositoryTests
	{
		private readonly IRepository<StudentFlattened> _sut;

		public CosmosDbRepositoryTests()
		{
			// this flag can be set to true for [few] tests. Running all the tests with this set to true might slow down.
			IntegrationTestHelper.IsCosmosDbServer = false;
			IntegrationTestHelper.IsSeedCosmosDbData = true;
			var serviceCollection = new ServiceCollection();
			serviceCollection
				.BoltOn(options =>
				{
					options
						.BoltOnCosmosDbModule();
				});
			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			_sut = serviceProvider.GetService<IRepository<StudentFlattened>>();
		}

		[Fact]
		public async Task DeleteAsync_DeleteById_DeletesTheEntity()
		{
			if (!IntegrationTestHelper.IsCosmosDbServer)
				return;

			// arrange
			var id = Guid.Parse("e2e1c2d0-9a7c-4659-b9e3-6760e740c26c");
			var student = new StudentFlattened { Id = id };

			// act
			await _sut.DeleteAsync(student, new RequestOptions { PartitionKey = new Microsoft.Azure.Documents.PartitionKey(2) });

			// assert
			var queryResult = _sut.GetById(id, new RequestOptions { PartitionKey = new Microsoft.Azure.Documents.PartitionKey(2) });
			Assert.Null(queryResult);
		}
	}
}
