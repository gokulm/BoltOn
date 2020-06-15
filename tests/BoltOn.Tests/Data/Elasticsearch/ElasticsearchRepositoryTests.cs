using System.Linq;
using System.Threading.Tasks;
using BoltOn.Data;
using BoltOn.Tests.Data.Elasticsearch.Fakes;
using BoltOn.Tests.Other;
using Microsoft.Extensions.DependencyInjection;
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
			var sut = _elasticDbFixture.ServiceProvider.GetService<IRepository<Student>>();

			// act
			await sut.DeleteAsync(10);

			// assert
			var queryResult = await sut.GetByIdAsync(id);
			Assert.Null(queryResult);
		}

		[Fact]
		public async Task GetByIdAsync_WhenRecordExists_ReturnsRecord()
		{
			// arrange
			if (!IntegrationTestHelper.IsElasticsearchServer)
				return;
			var id = 1;
			var sut = _elasticDbFixture.ServiceProvider.GetService<IRepository<Student>>();

			// act
			var result = await sut.GetByIdAsync(id);

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
			var sut = _elasticDbFixture.ServiceProvider.GetService<IRepository<Student>>();

			// act
			var result = await sut.GetByIdAsync(id);

			// assert
			Assert.Null(result);
		}

		[Fact]
		public async Task GetAllAsync_WhenRecordsExist_ReturnsRecords()
		{
			// arrange
			if (!IntegrationTestHelper.IsElasticsearchServer)
				return;
			var sut = _elasticDbFixture.ServiceProvider.GetService<IRepository<Student>>();

			// act
			System.Threading.Thread.Sleep(1000);
			var result = await sut.GetAllAsync();

			// assert
			Assert.True(result.Count() > 2);
		}

		//[Fact]
		//public async Task FindByAsync_WhenRecordDoesNotExist_ReturnsNull()
		//{
		//	// arrange
		//	if (!IntegrationTestHelper.IsElasticsearchServer)
		//		return;

		//	// act
		//	//var result = (await _sut.FindByAsync(f => f.StudentTypeId == 1 && f.FirstName == "johnny")).FirstOrDefault();

		//	//// assert
		//	//Assert.Null(result);
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

		//[Fact]
		//public async Task AddAsync_AddNewEntity_ReturnsAddedEntity()
		//{
		//	// arrange
		//	if (!IntegrationTestHelper.IsElasticsearchServer)
		//		return;
		//	var id = Guid.NewGuid();
		//	var employee = new Employee { Id = id, FirstName = "meghan", LastName = "doe" };

		//	//// act
		//	await _sut.AddAsync(employee);

		//	// assert
		//	var result = await _sut.GetByIdAsync(id);
		//	Assert.NotNull(result);
		//	Assert.Equal("meghan", result.FirstName);
		//}

		//[Fact]
		//public async Task AddAsync_AddNewEntities_ReturnsAddedEntities()
		//{
		//	// arrange
		//	if (!IntegrationTestHelper.IsElasticsearchServer)
		//		return;
		//	var id1 = Guid.NewGuid();
		//	var employee1 = new Employee { Id = id1, FirstName = "will", LastName = "smith" };
		//	var id2 = Guid.NewGuid();
		//	var employee2 = new Employee { Id = id2, FirstName = "john", LastName = "smith" };
		//	//var employees = new List<Employee> { employee1, employee2 };

		//	//// act
		//	//var actualResult = await _sut.AddAsync(employees);

		//	//// assert
		//	//var expectedResult = await _sut.GetAllAsync();
		//	//Assert.NotNull(actualResult);
		//	//Assert.Equal(expectedResult, actualResult);
		//}
	}
}
