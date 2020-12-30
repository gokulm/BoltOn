using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BoltOn.Tests.Data.EF.Fakes;
using BoltOn.Tests.Other;
using Xunit;

namespace BoltOn.Tests.Data.EF
{
	[Collection("IntegrationTests")]
	public class RepositoryTests : IClassFixture<EFRepositoryFixture>
	{
		private readonly EFRepositoryFixture _fixture;

		public RepositoryTests(EFRepositoryFixture fixture)
		{
			_fixture = fixture;
		}

		[Fact, Trait("Category", "Integration")]
		public async Task GetById_WhenRecordDoesNotExist_ReturnsNull()
		{
			// arrange

			// act
			var result = await _fixture.SubjectUnderTest.GetByIdAsync(3);

			// assert
			Assert.Null(result);
		}

		[Fact, Trait("Category", "Integration")]
		public async Task GetByIdAsync_WhenRecordExists_ReturnsRecord()
		{
			// arrange

			// act
			var result = await _fixture.SubjectUnderTest.GetByIdAsync(1);

			// assert
			Assert.NotNull(result);
			Assert.Equal("a", result.FirstName);
		}

		[Fact, Trait("Category", "Integration")]
		public async Task GetAllAsync_WhenRecordsExist_ReturnsAllTheRecords()
		{
			// arrange

			// act
			var result = await _fixture.SubjectUnderTest.GetAllAsync();

			// assert
			Assert.True(result.Count() > 4);
		}

		[Fact, Trait("Category", "Integration")]
		public async Task FindByWithoutIncludes_WhenRecordsExist_ReturnsRecordsThatMatchesTheFindByCriteria()
		{
			// arrange

			// act
			var result = (await _fixture.SubjectUnderTest.FindByAsync(f => f.Id == 2)).FirstOrDefault();

			// assert
			Assert.NotNull(result);
			Assert.Equal("x", result.FirstName);
		}

		[Fact, Trait("Category", "Integration")]
		public async Task FindByAsyncWithIncludes_WhenRecordsExist_ReturnsRecordsThatMatchesTheCriteria()
		{
			// act
			var result = (await _fixture.SubjectUnderTest.FindByAsync(f => f.Id == 2, default, i => i.Addresses)).FirstOrDefault();

			// assert
			Assert.NotNull(result);
			Assert.Equal("x", result.FirstName);
			Assert.NotEmpty(result.Addresses);
		}

		[Fact, Trait("Category", "Integration")]
		public async Task AddAsync_AddNewEntity_ReturnsAddedEntity()
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
			var result = await _fixture.SubjectUnderTest.AddAsync(student);
			var queryResult = await _fixture.SubjectUnderTest.GetByIdAsync(newStudentId);

			// assert
			Assert.NotNull(queryResult);
			Assert.Equal("c", queryResult.FirstName);
			Assert.Equal(result.FirstName, queryResult.FirstName);
		}

		[Fact, Trait("Category", "Integration")]
		public async Task AddAsync_AddNewEntities_ReturnsAddedEntities()
		{
			// arrange
			var student1 = new Student
			{
				Id = 7,
				FirstName = "a",
				LastName = "b"
			};
			var student2 = new Student
			{
				Id = 8,
				FirstName = "c",
				LastName = "d"
			};
			var students = new List<Student> { student1, student2 };
			var studentIds = students.Select(i => i.Id);

			// act
			var actualResult = await _fixture.SubjectUnderTest.AddAsync(students);
			var expectedResult = await _fixture.SubjectUnderTest.GetAllAsync();

			// assert
			Assert.NotNull(actualResult);
			Assert.Equal(expectedResult.Where(s => studentIds.Contains(s.Id)), actualResult);
		}

		[Fact, Trait("Category", "Integration")]
		public async Task UpdateAsync_UpdateAnExistingEntity_UpdatesTheEntity()
		{
			// arrange
			var student = await _fixture.SubjectUnderTest.GetByIdAsync(1);

			// act
			student.FirstName = "c";
			await _fixture.SubjectUnderTest.UpdateAsync(student);

			// assert
			var queryResult = await _fixture.SubjectUnderTest.GetByIdAsync(1);
			Assert.NotNull(queryResult);
			Assert.Equal("c", queryResult.FirstName);
		}

		[Fact, Trait("Category", "Integration")]
		public async Task DeleteAsync_DeleteAfterFetching_DeletesTheEntity()
		{
			// arrange
			var student = await _fixture.SubjectUnderTest.GetByIdAsync(10);

			// act
			await _fixture.SubjectUnderTest.DeleteAsync(10);

			// assert
			var queryResult = await _fixture.SubjectUnderTest.GetByIdAsync(10);
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
			await _fixture.SubjectUnderTest.DeleteAsync(student);
			var queryResult = await _fixture.SubjectUnderTest.GetByIdAsync(11);

			// assert
			Assert.Null(queryResult);
		}
	}
}
