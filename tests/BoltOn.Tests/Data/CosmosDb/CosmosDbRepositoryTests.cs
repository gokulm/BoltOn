using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BoltOn.Data;
using BoltOn.Tests.Data.CosmosDb.Fakes;
using BoltOn.Tests.Other;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Xunit;

namespace BoltOn.Tests.Data.CosmosDb
{

	[Collection("IntegrationTests")]
	public class CosmosDbRepositoryTests : IClassFixture<CosmosDbFixture>
	{
		private readonly CosmosDbFixture _cosmosDbFixture;

		public CosmosDbRepositoryTests(CosmosDbFixture cosmosDbFixture)
		{
			_cosmosDbFixture = cosmosDbFixture;
		}

		[Fact]
		public async Task DeleteAsync_DeleteById_DeletesTheEntity()
		{
			// arrange
			if (!IntegrationTestHelper.IsCosmosDbServer)
				return;
			var id = Guid.Parse("ff96d626-3911-4c78-b337-00d7ecd2eadd");

			// act
			await _cosmosDbFixture.SubjectUnderTest.DeleteAsync(id,
				new RequestOptions { PartitionKey = new PartitionKey(1) });

			// assert
			var queryResult = await _cosmosDbFixture.SubjectUnderTest.GetByIdAsync(id, new RequestOptions { PartitionKey = new PartitionKey(1) });
			Assert.Null(queryResult);
		}

		[Fact]
		public async Task GetByIdAsync_WhenRecordExists_ReturnsRecord()
		{
			// arrange
			if (!IntegrationTestHelper.IsCosmosDbServer)
				return;
			var id = Guid.Parse("eda6ac19-0b7c-4698-a1f7-88279339d9ff");

			// act
			var result = await _cosmosDbFixture.SubjectUnderTest.GetByIdAsync(id, new RequestOptions { PartitionKey = new PartitionKey(1) });

			// assert
			Assert.NotNull(result);
			Assert.Equal("john", result.FirstName);
		}

		[Fact]
		public async Task GetAllAsync_WhenRecordsExist_ReturnsAllTheRecords()
		{
			// arrange
			if (!IntegrationTestHelper.IsCosmosDbServer)
				return;

			// act
			var result = await _cosmosDbFixture.SubjectUnderTest.GetAllAsync();

			// assert
			Assert.True(result.Count() > 1);
		}

		[Fact]
		public async Task FindByAsync_WhenRecordDoesNotExist_ReturnsNull()
		{
			// arrange
			if (!IntegrationTestHelper.IsCosmosDbServer)
				return;

			// act
			var result = (await _cosmosDbFixture.SubjectUnderTest.FindByAsync(f => f.StudentTypeId == 1 && f.FirstName == "johnny")).FirstOrDefault();

			// assert
			Assert.Null(result);
		}

		[Fact]
		public async Task FindByAsync_WhenRecordExist_ReturnsRecord()
		{
			// arrange
			if (!IntegrationTestHelper.IsCosmosDbServer)
				return;

			// act
			var result = (await _cosmosDbFixture.SubjectUnderTest.FindByAsync(f => f.StudentTypeId == 1 && f.FirstName == "john")).FirstOrDefault();

			// assert
			Assert.NotNull(result);
		}

		[Fact]
		public async Task UpdateAsync_UpdateAnExistingEntity_UpdatesTheEntity()
		{
			// arrange
			if (!IntegrationTestHelper.IsCosmosDbServer)
				return;
			var id = Guid.Parse("eda6ac19-0b7c-4698-a1f7-88279339d9ff");
			var student = new StudentFlattened { Id = id, LastName = "smith jr", FirstName = "john", StudentTypeId = 1 };

			// act
			await _cosmosDbFixture.SubjectUnderTest.UpdateAsync(student, new RequestOptions { PartitionKey = new PartitionKey(1) });

			// assert
			var result = (await _cosmosDbFixture.SubjectUnderTest.FindByAsync(f => f.StudentTypeId == 1 && f.FirstName == "john")).FirstOrDefault();
			Assert.NotNull(result);
			Assert.Equal("smith jr", result.LastName);
		}

		[Fact]
		public async Task AddAsync_AddNewEntity_ReturnsAddedEntity()
		{
			// arrange
			if (!IntegrationTestHelper.IsCosmosDbServer)
				return;
			var id = Guid.NewGuid();
			var studentFlattened = new StudentFlattened { Id = id, StudentTypeId = 2, FirstName = "meghan", LastName = "doe" };

			// act
			await _cosmosDbFixture.SubjectUnderTest.AddAsync(studentFlattened);

			// assert
			var result = await _cosmosDbFixture.SubjectUnderTest.GetByIdAsync(id, new RequestOptions { PartitionKey = new PartitionKey(2) });
			Assert.NotNull(result);
			Assert.Equal("meghan", result.FirstName);
		}

		[Fact]
		public async Task AddAsync_AddNewEntities_ReturnsAddedEntities()
		{
			// arrange
			if (!IntegrationTestHelper.IsCosmosDbServer)
				return;
			var id1 = Guid.NewGuid();
			var student1Flattened = new StudentFlattened { Id = id1, StudentTypeId = 2, FirstName = "meghan", LastName = "doe" };
			var id2 = Guid.NewGuid();
			var student2Flattened = new StudentFlattened { Id = id2, StudentTypeId = 2, FirstName = "john", LastName = "smith" };
			var studentsFlattened = new List<StudentFlattened> { student1Flattened, student2Flattened };

			// act
			var actualResult = await _cosmosDbFixture.SubjectUnderTest.AddAsync(studentsFlattened, new RequestOptions { PartitionKey = new PartitionKey(2) });

			// assert
			var expectedResult = await _cosmosDbFixture.SubjectUnderTest.GetAllAsync(new RequestOptions { PartitionKey = new PartitionKey(2) });
			Assert.NotNull(actualResult);
			Assert.Equal(expectedResult, actualResult);
		}

		public void Dispose()
		{
			//if (IntegrationTestHelper.IsCosmosDbServer)
			//{
			//	var id = Guid.Parse("eda6ac19-0b7c-4698-a1f7-88279339d9ff");
			//	_sut.DeleteAsync(id, new RequestOptions { PartitionKey = new PartitionKey(1) }).GetAwaiter().GetResult();
			//}
		}
	}
}
