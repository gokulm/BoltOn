using System;
using BoltOn.Data.EF;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using System.Threading.Tasks;
using System.Linq;
using BoltOn.Tests.Other;
using System.Linq.Expressions;
using System.Collections.Generic;
using BoltOn.Data;

namespace BoltOn.Tests.Data.EF
{
	// Collection is used to prevent running tests in parallel i.e., tests in the same collection
	// will not be executed in parallel
	[Collection("IntegrationTests")]
	public class EFRepositoryTests 
	{
		private readonly IRepository<Student> _sut;

		public EFRepositoryTests()
		{
			// this flag can be set to true for [few] tests. Running all the tests with this set to true might slow down.
			IntegrationTestHelper.IsSqlServer = false;
			IntegrationTestHelper.IsSeedData = true;
			var serviceCollection = new ServiceCollection();
			serviceCollection
				.BoltOn(options =>
				{
					options
						.BoltOnEFModule();
				});
			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			_sut = serviceProvider.GetService<IRepository<Student>>();
		}

		[Fact, Trait("Category", "Integration")]
		public async Task GetById_WhenRecordExists_ReturnsRecord()
		{
			// arrange

			// act
			var result = await _sut.GetByIdAsync(1);

			// assert
			Assert.NotNull(result);
			Assert.Equal("a", result.FirstName);
		}

		[Fact, Trait("Category", "Integration")]
		public async Task GetById_WhenRecordDoesNotExist_ReturnsNull()
		{
			// arrange

			// act
			var result = await _sut.GetByIdAsync(3);

			// assert
			Assert.Null(result);
		}

		[Fact, Trait("Category", "Integration")]
		public async Task GetByIdAsync_WhenRecordExists_ReturnsRecord()
		{
			// arrange

			// act
			var result = await _sut.GetByIdAsync(1);

			// assert
			Assert.NotNull(result);
			Assert.Equal("a", result.FirstName);
		}

		[Fact, Trait("Category", "Integration")]
		public async Task GetAll_WhenRecordsExist_ReturnsAllTheRecords()
		{
			// arrange

			// act
			var result = (await _sut.GetAllAsync()).ToList();

			// assert
			Assert.Equal(4, result.Count);
		}

		[Fact, Trait("Category", "Integration")]
		public async Task GetAllAsync_WhenRecordsExist_ReturnsAllTheRecords()
		{
			// arrange

			// act
			var result = await _sut.GetAllAsync();

			// assert
			Assert.Equal(4, result.Count());
		}

		[Fact, Trait("Category", "Integration")]
		public async Task FindByWithoutIncludes_WhenRecordsExist_ReturnsRecordsThatMatchesTheFindByCriteria()
		{
			// arrange

			// act
			var result = (await _sut.FindByAsync(f => f.Id == 2)).FirstOrDefault();

			// assert
			Assert.NotNull(result);
			Assert.Equal("x", result.FirstName);
		}

		[Fact, Trait("Category", "Integration")]
		public async Task FindByWithIncludes_WhenRecordsExist_ReturnsRecordsThatMatchesTheCriteria()
		{
			// arrange
			var includes = new List<Expression<Func<Student, object>>>
			{
				s => s.Addresses
			};

			// act
			var result = (await _sut.FindByAsync(f => f.Id == 2, includes)).FirstOrDefault();

			// assert
			Assert.NotNull(result);
			Assert.Equal("x", result.FirstName);
			Assert.NotEmpty(result.Addresses);
		}

		[Fact, Trait("Category", "Integration")]
		public async Task FindByAsyncWithIncludes_WhenRecordsExist_ReturnsRecordsThatMatchesTheCriteria()
		{
			// arrange
			var includes = new List<Expression<Func<Student, object>>>
			{
				s => s.Addresses
			};

			// act
			var result = (await _sut.FindByAsync(f => f.Id == 2, includes, default)).FirstOrDefault();

			// assert
			Assert.NotNull(result);
			Assert.Equal("x", result.FirstName);
			Assert.NotEmpty(result.Addresses);
		}

		[Fact, Trait("Category", "Integration")]
		public async Task Add_AddANewEntity_ReturnsAddedEntity()
		{
			// arrange
			const int newStudentId = 5;
			var student = new Student
			{
				Id = newStudentId,
				FirstName = "c",
				LastName = "d"
			};

			// act
			var result = await _sut.AddAsync(student);
			var queryResult = await _sut.GetByIdAsync(newStudentId);

			// assert
			Assert.NotNull(queryResult);
			Assert.Equal("c", queryResult.FirstName);
			Assert.Equal(result.FirstName, queryResult.FirstName);
		}

		[Fact, Trait("Category", "Integration")]
		public async Task AddAsync_AddANewEntity_ReturnsAddedEntity()
		{
			// arrange
			const int newStudentId = 6;
			var student = new Student
			{
				Id = newStudentId,
				FirstName = "c",
				LastName = "d"
			};

			// act
			var result = await _sut.AddAsync(student);
			var queryResult = await _sut.GetByIdAsync(newStudentId);

			// assert
			Assert.NotNull(queryResult);
			Assert.Equal("c", queryResult.FirstName);
			Assert.Equal(result.FirstName, queryResult.FirstName);
		}

		[Fact, Trait("Category", "Integration")]
		public async Task AddAsync_AddANewEntities_ReturnsAddedEntities()
		{
			// arrange
			var student1 = new Student
			{
				Id = 5,
				FirstName = "a",
				LastName = "b"
			};
			var student2 = new Student
			{
				Id = 6,
				FirstName = "c",
				LastName = "d"
			};
			var students = new List<Student> { student1, student2 };
			var studentIds = students.Select(i => i.Id);

			// act
			var actualResult = await _sut.AddAsync(students);
			var expectedResult = await _sut.GetAllAsync();

			// assert
			Assert.NotNull(actualResult);
			Assert.Equal(expectedResult.Where(s => studentIds.Contains(s.Id)), actualResult);
		}

		[Fact, Trait("Category", "Integration")]
		public async Task Update_UpdateAnExistingEntity_UpdatesTheEntity()
		{
			// arrange
			var student = await _sut.GetByIdAsync(2);

			// act
			student.FirstName = "c";
			await _sut.UpdateAsync(student);
			var queryResult = await _sut.GetByIdAsync(2);

			// assert
			Assert.NotNull(queryResult);
			Assert.Equal("c", queryResult.FirstName);
		}

		[Fact, Trait("Category", "Integration")]
		public async Task UpdateAsync_UpdateAnExistingEntity_UpdatesTheEntity()
		{
			// arrange
			var student = await _sut.GetByIdAsync(2);

			// act
			student.FirstName = "c";
			await _sut.UpdateAsync(student);

			// assert
			var queryResult = await _sut.GetByIdAsync(2);
			Assert.NotNull(queryResult);
			Assert.Equal("c", queryResult.FirstName);
		}

		[Fact, Trait("Category", "Integration")]
		public async Task DeleteAsync_DeleteAfterFetching_DeletesTheEntity()
		{
			// arrange
			var student = await _sut.GetByIdAsync(10);

			// act
			await _sut.DeleteAsync(student);

			// assert
			var queryResult = await _sut.GetByIdAsync(10);
			Assert.Null(queryResult);
		}

		[Fact, Trait("Category", "Integration")]
		public async Task DeleteAsync_DeleteById_DeletesTheEntity()
		{
			if (!IntegrationTestHelper.IsSqlServer)
				return;

			// arrange
			var student = new Student { Id = 11 };

			// act
			await _sut.DeleteAsync(student);
			var queryResult = await _sut.GetByIdAsync(11);

			// assert
			Assert.Null(queryResult);
		}
	}
}
