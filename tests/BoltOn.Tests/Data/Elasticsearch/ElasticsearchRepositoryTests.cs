using System;
using System.Threading.Tasks;
using BoltOn.Data;
using BoltOn.Data.Elasticsearch;
using BoltOn.Tests.Data.Elasticsearch.Fakes;
using BoltOn.Tests.Other;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BoltOn.Tests.Data.Elasticsearch
{
	[Collection("IntegrationTests")]
	public class ElasticsearchRepositoryTests : IDisposable
	{
		private readonly IRepository<Person> _sut;

		public ElasticsearchRepositoryTests()
		{
			IntegrationTestHelper.IsElasticsearchServer = true;
			IntegrationTestHelper.IsSeedElasticsearch = true;

			var serviceCollection = new ServiceCollection();
			serviceCollection
				.BoltOn(options =>
				{
					options.BoltOnElasticsearchModule();
					options.SetupFakes();
				});

			serviceCollection.AddElasticsearch<TestElasticsearchOptions>(
				t => t.ConnectionSettings = new Nest.ConnectionSettings(new Uri("http://127.0.0.1:9200")));
			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();

			if (!IntegrationTestHelper.IsElasticsearchServer)
				return;

			_sut = serviceProvider.GetService<IRepository<Person>>();
		}

		[Fact]
		public async Task DeleteAsync_DeleteById_DeletesTheEntity()
		{
			// arrange
			if (!IntegrationTestHelper.IsElasticsearchServer)
				return;
			var id = Guid.Parse("84d19d30-a395-4983-b0fa-4caff052eacb");
			var person = new Person { Id = id };

			// act
		   await _sut.DeleteAsync(person);

			// assert
			var queryResult = await _sut.GetByIdAsync(id);
			Assert.Null(queryResult);
		}

		//[Fact]
		//public async Task GetByIdAsync_WhenRecordExists_ReturnsRecord()
		//{
		//	// arrange
		//	if (!IntegrationTestHelper.IsElasticsearchServer)
		//		return;
		//	var id = Guid.Parse("eda6ac19-0b7c-4698-a1f7-88279339d9ff");

		//	// act
		//	//var result = await _sut.GetByIdAsync(id, new RequestOptions { PartitionKey = new PartitionKey(1) });

		//	//// assert
		//	//Assert.NotNull(result);
		//	//Assert.Equal("john", result.FirstName);
		//}

		//[Fact]
		//public async Task GetAllAsync_WhenRecordsExist_ReturnsAllTheRecords()
		//{
		//	// arrange
		//	if (!IntegrationTestHelper.IsElasticsearchServer)
		//		return;

		//	// act
		//	var result = await _sut.GetAllAsync();

		//	// assert
		//	//Assert.True(result.Count() > 1);
		//}

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

		public void Dispose()
		{
			if (IntegrationTestHelper.IsElasticsearchServer)
			{
				var id = Guid.Parse("eda6ac19-0b7c-4698-a1f7-88279339d9ff");
				//var student = new StudentFlattened { Id = id };
				//_sut.DeleteAsync(student, new RequestOptions { PartitionKey = new PartitionKey(1) }).GetAwaiter().GetResult();
			}
		}
	}
}
