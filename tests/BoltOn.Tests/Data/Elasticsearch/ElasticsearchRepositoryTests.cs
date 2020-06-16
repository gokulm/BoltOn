using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BoltOn.Data;
using BoltOn.Tests.Data.Elasticsearch.Fakes;
using BoltOn.Tests.Other;
using Microsoft.Extensions.DependencyInjection;
using Nest;
using Xunit;

namespace BoltOn.Tests.Data.Elasticsearch
{
	[Collection("IntegrationTests")]
	public class ElasticsearchRepositoryTests : IClassFixture<ElasticDbFixture>
	{
		private readonly ElasticDbFixture _elasticDbFixture;

		public ElasticsearchRepositoryTests(ElasticDbFixture elasticDbFixture)
		{
			_elasticDbFixture = elasticDbFixture;
		}

		[Fact]
		public async Task DeleteAsync_DeleteById_DeletesTheEntity()
		{
			// arrange
			if (!IntegrationTestHelper.IsElasticsearchServer)
				return;
			var id = 10;

			// act
			await _elasticDbFixture.SubjectUnderTest.DeleteAsync(10);

			// assert
			var queryResult = await _elasticDbFixture.SubjectUnderTest.GetByIdAsync(id);
			Assert.Null(queryResult);
		}

		[Fact]
		public async Task GetByIdAsync_WhenRecordExists_ReturnsRecord()
		{
			// arrange
			if (!IntegrationTestHelper.IsElasticsearchServer)
				return;
			var id = 1;

			// act
			var result = await _elasticDbFixture.SubjectUnderTest.GetByIdAsync(id);

			// assert
			Assert.NotNull(result);
			Assert.Equal("John", result.FirstName);
		}

		[Fact]
		public async Task GetByIdAsync_WhenRecordDoesNotExist_ReturnsNull()
		{
			// arrange
			if (!IntegrationTestHelper.IsElasticsearchServer)
				return;
			var id = 3;

			// act
			var result = await _elasticDbFixture.SubjectUnderTest.GetByIdAsync(id);

			// assert
			Assert.Null(result);
		}

		[Fact]
		public async Task GetAllAsync_WhenRecordsExist_ReturnsRecords()
		{
			// arrange
			if (!IntegrationTestHelper.IsElasticsearchServer)
				return;

			// act
			System.Threading.Thread.Sleep(1000);
			var result = await _elasticDbFixture.SubjectUnderTest.GetAllAsync();

			// assert
			Assert.True(result.Count() > 2);
		}

		//[Fact]
		//public async Task FindByAsync_WhenRecordDoesNotExist_ReturnsNull()
		//{
		//	// arrange
		//	if (!IntegrationTestHelper.IsElasticsearchServer)
		//		return;
		//	var sut = _elasticDbFixture.ServiceProvider.GetService<BoltOn.Data.IRepository<Student>>();
		//	var searchRequest = new SearchRequest<Student>
		//	{
		//		Query = new TermQuery
		//		{
		//			Field = "firstName",
		//			Value = "John"
		//		}
		//	};

		//	// act
		//	var result = (await sut.FindByAsync(null, searchRequest)).FirstOrDefault();

		//	// assert
		//	Assert.Null(result);
		//}

		//[Fact]
		//public async Task FindByAsync_WhenRecordExist_ReturnsRecord()
		//{
		//	// arrange
		//	if (!IntegrationTestHelper.IsElasticsearchServer)
		//		return;

		//	// act
		//	//var result = (await _sut.FindByAsync(f => f.StudentTypeId == 1 && f.FirstName == "john")).FirstOrDefault();

		//	//// assert
		//	//Assert.NotNull(result);
		//}

		//[Fact]
		//public async Task UpdateAsync_UpdateAnExistingEntity_UpdatesTheEntity()
		//{
		//	// arrange
		//	if (!IntegrationTestHelper.IsElasticsearchServer)
		//		return;
		//	var id = Guid.Parse("eda6ac19-0b7c-4698-a1f7-88279339d9ff");
		//	//var student = new Employee { Id = id, LastName = "smith jr", FirstName = "john", StudentTypeId = 1 };

		//	// act
		//	//await _sut.UpdateAsync(student, new RequestOptions { PartitionKey = new PartitionKey(1) });

		//	// assert
		//	//var result = (await _sut.FindByAsync(f => f.StudentTypeId == 1 && f.FirstName == "john")).FirstOrDefault();
		//	//Assert.NotNull(result);
		//	//Assert.Equal("smith jr", result.LastName);
		//}

		[Fact]
		public async Task AddAsync_AddNewEntity_ReturnsAddedEntity()
		{
			// arrange
			if (!IntegrationTestHelper.IsElasticsearchServer)
				return;
			var id = 4;
			var student = new Student { Id = id, FirstName = "meghan", LastName = "doe" };

			// act
			await _elasticDbFixture.SubjectUnderTest.AddAsync(student);

			// assert
			var result = await _elasticDbFixture.SubjectUnderTest.GetByIdAsync(id);
			Assert.NotNull(result);
			Assert.Equal("meghan", result.FirstName);
		}

		[Fact]
		public async Task AddAsync_AddNewEntities_ReturnsAddedEntities()
		{
			// arrange
			if (!IntegrationTestHelper.IsElasticsearchServer)
				return;
			var id1 = 5;
			var student1 = new Student { Id = id1, FirstName = "will", LastName = "smith" };
			var id2 = 6;
			var student2 = new Student { Id = id2, FirstName = "brad", LastName = "pitt" };
			var employees = new List<Student> { student1, student2 };

			// act
			var actualResult = await _elasticDbFixture.SubjectUnderTest.AddAsync(employees);

			// assert
			System.Threading.Thread.Sleep(1000);
			var expectedResult = await _elasticDbFixture.SubjectUnderTest.GetAllAsync();
			Assert.NotNull(actualResult);
			Assert.True(expectedResult.Any(a => a.FirstName == "will" && a.LastName == "smith"));
			Assert.True(expectedResult.Any(a => a.FirstName == "brad" && a.LastName == "pitt"));
		}
	}
}
