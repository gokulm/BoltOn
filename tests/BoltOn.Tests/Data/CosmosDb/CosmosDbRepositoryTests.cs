using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using BoltOn.Tests.Common;
using BoltOn.Tests.Data.CosmosDb.Fakes;
using BoltOn.Tests.Other;
using Microsoft.Azure.Cosmos;
using Xunit;

namespace BoltOn.Tests.Data.CosmosDb
{
	[Collection("IntegrationTests")]
	[TestCaseOrderer("BoltOn.Tests.Common.PriorityOrderer", "BoltOn.Tests")]
	public class CosmosDbRepositoryTests : IClassFixture<CosmosDbFixture>
	{
		private readonly CosmosDbFixture _cosmosDbFixture;
		private static Guid _studentId = Guid.NewGuid();

		public CosmosDbRepositoryTests(CosmosDbFixture cosmosDbFixture)
		{
			_cosmosDbFixture = cosmosDbFixture;
		}

		[Fact]
		[TestPriority(1)]
		public async Task AddAsync_AddNewEntity_ReturnsAddedEntity()
		{
			// arrange
			if (!IntegrationTestHelper.IsCosmosDbServer)
				return;
			var student = new Fakes.Student { Id = _studentId, FirstName = "meghan", LastName = "doe", CourseId = "1", Age = 20 };

			// act
			await _cosmosDbFixture.SubjectUnderTest.AddAsync(student);

			// assert
			var result = (await _cosmosDbFixture.SubjectUnderTest.FindByAsync(f => f.Id == _studentId)).FirstOrDefault();
			Assert.NotNull(result);
			Assert.Equal("meghan", result.FirstName);
		}

		[Fact]
		[TestPriority(2)]
		public async Task GetAllAsync_WhenRecordsExist_ReturnsRecords()
		{
			// arrange
			if (!IntegrationTestHelper.IsCosmosDbServer)
				return;
			var id = Guid.NewGuid();
			var student = new Fakes.Student
			{
				Id = id,
				FirstName = "john",
				LastName = "smith",
				CourseId = "1",
				Addresses = new List<Fakes.Address>
				{
					new Fakes.Address { Id = Guid.NewGuid(), City ="a", Street = "b"},
					new Fakes.Address { Id = Guid.NewGuid(), City ="x", Street = "y"}
				}
			};
			await _cosmosDbFixture.SubjectUnderTest.AddAsync(student);

			// act
			var result = await _cosmosDbFixture.SubjectUnderTest.GetAllAsync();

			// assert
			Assert.True(result.Count() == 2);
		}

		[Fact]
		[TestPriority(3)]
		public async Task FindByAsync_WhenExactMatch_ReturnsRecord()
		{
			// arrange
			if (!IntegrationTestHelper.IsCosmosDbServer)
				return;

			// act
			var result = await _cosmosDbFixture.SubjectUnderTest
					.FindByAsync(f => f.FirstName.Equals("john"));

			// assert
			Assert.NotNull(result);
			Assert.True(result.Count() == 1);
			Assert.Equal("john", result.First().FirstName);
		}

		[Fact]
		[TestPriority(3)]
		public async Task FindByAsync_ChildNodeSearch_ReturnsRecord()
		{
			// arrange
			if (!IntegrationTestHelper.IsCosmosDbServer)
				return;

			// act
			var result = await _cosmosDbFixture.SubjectUnderTest
					.FindByAsync(f => f.Addresses.Any(w => w.City == "a"));

			// assert
			Assert.NotNull(result);
			Assert.True(result.Count() == 1);
			Assert.Equal("john", result.First().FirstName);
		}

		[Fact]
		[TestPriority(3)]
		public async Task FindByAsync_CaseInsensitive_ReturnsRecord()
		{

			// arrange
			if (!IntegrationTestHelper.IsCosmosDbServer)
				return;

			// act
			var result = await _cosmosDbFixture.SubjectUnderTest
					.FindByAsync(f => f.FirstName.Equals("John", StringComparison.InvariantCultureIgnoreCase));

			// assert
			Assert.NotNull(result);
			Assert.True(result.Count() == 1);
			Assert.Equal("john", result.First().FirstName);
		}

		[Fact]
		[TestPriority(3)]
		public async Task FindByAsync_WhenContainsMatch_ReturnRecord()
		{
			// arrange
			if (!IntegrationTestHelper.IsCosmosDbServer)
				return;

			// act
			var result = await _cosmosDbFixture.SubjectUnderTest
					.FindByAsync(f => f.FirstName.Contains("jo"));

			// assert
			Assert.NotNull(result);
			Assert.True(result.Count() == 1);
			Assert.Equal("john", result.First().FirstName);
		}

		[Fact]
		[TestPriority(3)]
		public async Task FindByAsync_AndSearch_ReturnRecord()
		{
			// arrange
			if (!IntegrationTestHelper.IsCosmosDbServer)
				return;

			// act
			var result = await _cosmosDbFixture.SubjectUnderTest
					.FindByAsync(f => f.FirstName.Contains("jo") && f.LastName.Equals("smith"));

			// assert
			Assert.NotNull(result);
			Assert.True(result.Count() == 1);
			Assert.Equal("john", result.First().FirstName);
		}

		[Fact]
		[TestPriority(4)]
		public async Task UpdateAsync_UpdateAnExistingEntity_UpdatesTheEntity()
		{
			// arrange
			if (!IntegrationTestHelper.IsCosmosDbServer)
				return;
			//var student = new Fakes.Student { LastName = "smith jr" };
			var student = (await _cosmosDbFixture.SubjectUnderTest.FindByAsync(f => f.Id == _studentId)).FirstOrDefault();
			student.LastName = "smith jr";

			// act
			await _cosmosDbFixture.SubjectUnderTest.UpdateAsync(student, _studentId.ToString());

			// assert
			var result = (await _cosmosDbFixture.SubjectUnderTest.FindByAsync(f => f.Id == _studentId)).FirstOrDefault();
			Assert.NotNull(result);
			Assert.Equal("smith jr", result.LastName);
		}

		[Fact]
		[TestPriority(5)]
		public async Task AddAsync_AddNewEntities_ReturnsAddedEntities()
		{
			// arrange
			if (!IntegrationTestHelper.IsCosmosDbServer)
				return;
			var student1 = new Fakes.Student { Id = Guid.NewGuid(), FirstName = "will", LastName = "smith", CourseId = "1" };
			var student2 = new Fakes.Student { Id = Guid.NewGuid(), FirstName = "brad", LastName = "pitt", CourseId = "1" };
			var students = new List<Fakes.Student> { student1, student2 };

			// act
			var expectedResult = await _cosmosDbFixture.SubjectUnderTest.AddAsync(students, new PartitionKey("1"));

			// assert
			Assert.NotNull(expectedResult.FirstOrDefault(a => a.FirstName == "will" && a.LastName == "smith"));
			Assert.NotNull(expectedResult.FirstOrDefault(a => a.FirstName == "brad" && a.LastName == "pitt"));
		}

		[Fact]
		[TestPriority(6)]
		public async Task PatchAsync_WithPatchOperations_PatchesTheEntity()
		{
			// arrange
			if (!IntegrationTestHelper.IsCosmosDbServer)
				return;
			List<PatchOperation> patchOperations = new()
			{
				PatchOperation.Add("/MiddleName", "M"),
				PatchOperation.Remove("/LastName"),
				PatchOperation.Increment("/Age", 25)
			};

			// act
			await _cosmosDbFixture.SubjectUnderTest.PatchAsync(_studentId.ToString(), new PartitionKey("1"), patchOperations);

			// assert
			// await Task.Delay(2000);
			var result = (await _cosmosDbFixture.SubjectUnderTest.FindByAsync(f => f.Id == _studentId)).FirstOrDefault();
			Assert.NotNull(result);
			Assert.Equal(45, result.Age);
			Assert.Null(result.LastName);
			// todo: assert patch operations
			// var studentString = JsonSerializer.Serialize(result);
			// Assert.Contains("MiddleName", studentString);
		}

		[Fact]
		[TestPriority(10)]
		public async Task DeleteAsync_DeleteById_DeletesTheEntity()
		{
			// arrange
			if (!IntegrationTestHelper.IsCosmosDbServer)
				return;

			// act
			await _cosmosDbFixture.SubjectUnderTest.DeleteAsync(_studentId.ToString(), new PartitionKey("1"));

			// assert
			var result = (await _cosmosDbFixture.SubjectUnderTest.FindByAsync(f => f.Id == _studentId)).FirstOrDefault();
			Assert.Null(result);
		}
	}
}
