using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BoltOn.Tests.Common;
using BoltOn.Tests.Data.CosmosDb.Fakes;
using BoltOn.Tests.Other;
using Microsoft.Extensions.DependencyInjection;
using Nest;
using Xunit;

namespace BoltOn.Tests.Data.CosmosDb
{
	[Collection("IntegrationTests")]
	public class CosmosDbRepositoryTests : IClassFixture<CosmosDbFixture>
	{
		private readonly CosmosDbFixture _cosmosDbFixture;
		private Guid _studentId = Guid.NewGuid();

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
			var student = new Fakes.Student { Id = _studentId, FirstName = "meghan", LastName = "doe", CourseId = 1 };

			// act
			await _cosmosDbFixture.SubjectUnderTest.AddAsync(student);

			// assert
			var result = (await _cosmosDbFixture.SubjectUnderTest.FindByAsync(f => f.Id == _studentId)).FirstOrDefault();
			Assert.NotNull(result);
			Assert.Equal("meghan", result.FirstName);
		}

		[Fact]
		public async Task GetAllAsync_WhenRecordsExist_ReturnsRecords()
		{
			// arrange
			if (!IntegrationTestHelper.IsCosmosDbServer)
				return;
			var id = Guid.NewGuid();
			var student = new Fakes.Student { Id = id, FirstName = "john", LastName = "smith", CourseId = 1 };
			await _cosmosDbFixture.SubjectUnderTest.AddAsync(student);

			// act
			var result = await _cosmosDbFixture.SubjectUnderTest.GetAllAsync();

			// assert
			Assert.True(result.Count() > 1);
		}

		[Fact]
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

		//[Fact]
		//public async Task FindByAsync_ChildNodeSearch_ReturnsRecord()
		//{
		//	// arrange
		//	if (!IntegrationTestHelper.IsElasticsearchServer)
		//		return;
		//	var searchRequest = new SearchRequest
		//	{
		//		Query = new MatchQuery { Field = "addresses.street", Query = "sparkman" }
		//	};

		//	// act
		//	var result = await Search(searchRequest);

		//	// assert
		//	Assert.NotNull(result);
		//	Assert.True(result.Count() == 1);
		//	Assert.Equal("Jaden", result.First().FirstName);
		//}

		[Fact]
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
		public async Task FindByAsync_WhenContainsMatch_ReturnRecord()
		{
			// arrange
			if (!IntegrationTestHelper.IsCosmosDbServer)
				return;

			// act
			var result = await _cosmosDbFixture.SubjectUnderTest
					.FindByAsync(f => f.FirstName.StartsWith("jo"));

			// assert
			Assert.NotNull(result);
			Assert.True(result.Count() == 1);
			Assert.Equal("john", result.First().FirstName);
		}

		//[Fact]
		//public async Task FindByAsync_AndSearch_ReturnRecord()
		//{
		//	// arrange
		//	if (!IntegrationTestHelper.IsElasticsearchServer)
		//		return;
		//	var searchRequest = new SearchRequest
		//	{
		//		Query = new MatchQuery { Field = "firstName", Query = "John" } &&
		//		new MatchQuery { Field = "lastName", Query = "Smith" }
		//	};

		//	// act
		//	var result = await Search(searchRequest);

		//	// assert
		//	Assert.NotNull(result);
		//	Assert.True(result.Count() == 1);
		//}

		//[Fact]
		//public async Task FindByAsync_WhenNoMatch_ReturnNull()
		//{
		//	// arrange
		//	if (!IntegrationTestHelper.IsElasticsearchServer)
		//		return;
		//	var searchRequest = new SearchRequest
		//	{
		//		Query = new MatchQuery { Field = "firstName", Query = "2" }
		//	};

		//	// act
		//	var result = await Search(searchRequest);

		//	// assert
		//	Assert.NotNull(result);
		//	Assert.True(result.Count() == 0);
		//}

		//[Fact]
		//public async Task AddAsync_AddNewEntities_ReturnsAddedEntities()
		//{
		//	// arrange
		//	if (!IntegrationTestHelper.IsElasticsearchServer)
		//		return;
		//	var id1 = 5;
		//	var student1 = new Student { Id = id1, FirstName = "will", LastName = "smith" };
		//	var id2 = 6;
		//	var student2 = new Student { Id = id2, FirstName = "brad", LastName = "pitt" };
		//	var employees = new List<Student> { student1, student2 };

		//	// act
		//	var actualResult = await _elasticDbFixture.SubjectUnderTest.AddAsync(employees);

		//	// assert
		//	System.Threading.Thread.Sleep(1000);
		//	var expectedResult = await _elasticDbFixture.SubjectUnderTest.GetAllAsync();
		//	Assert.NotNull(actualResult);
		//	Assert.NotNull(expectedResult.FirstOrDefault(a => a.FirstName == "will" && a.LastName == "smith"));
		//	Assert.NotNull(expectedResult.FirstOrDefault(a => a.FirstName == "brad" && a.LastName == "pitt"));
		//}

		[Fact]
		[TestPriority(2)]
		public async Task UpdateAsync_UpdateAnExistingEntity_UpdatesTheEntity()
		{
			// arrange
			if (!IntegrationTestHelper.IsCosmosDbServer)
				return;
			var student = new Fakes.Student { LastName = "smith jr" };

			// act
			await _cosmosDbFixture.SubjectUnderTest.UpdateAsync(student, _studentId.ToString());

			// assert
			var result = (await _cosmosDbFixture.SubjectUnderTest.FindByAsync(f => f.Id == _studentId)).FirstOrDefault();
			Assert.NotNull(result);
			Assert.Equal("smith jr", result.LastName);
		}

		[Fact]
		public async Task DeleteAsync_DeleteById_DeletesTheEntity()
		{
			// arrange
			if (!IntegrationTestHelper.IsCosmosDbServer)
				return;

			// act
			await _cosmosDbFixture.SubjectUnderTest.DeleteAsync(_studentId.ToString(), "1");

			// assert
			var result = (await _cosmosDbFixture.SubjectUnderTest.FindByAsync(f => f.Id == _studentId)).FirstOrDefault();
			Assert.Null(result);
		}
	}
}
