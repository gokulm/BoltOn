using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BoltOn.Data.MartenDb;
using BoltOn.Tests.Common;
using BoltOn.Tests.Data.MartenDb.Fakes;
using BoltOn.Tests.Other;
using Xunit;
using Student = BoltOn.Tests.Data.MartenDb.Fakes.Student;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.Tests.Data.MartenDb
{
    [Collection("IntegrationTests")]
	[TestCaseOrderer("BoltOn.Tests.Common.PriorityOrderer", "BoltOn.Tests")]
	public class RepositoryTests : IClassFixture<MartenDbRepositoryFixture>
    {
        private readonly MartenDbRepositoryFixture _fixture;

        public RepositoryTests(MartenDbRepositoryFixture fixture)
        {
            _fixture = fixture;
        }

		[Fact, Trait("Category", "Integration")]
		[TestPriority(10)]
		public async Task GetByIdAsync_WhenRecordDoesNotExist_ReturnsNull()
		{
			if (!IntegrationTestHelper.IsMartenDbRunning)
				return;

			// arrange

			// act
			var result = await _fixture.SubjectUnderTest.GetByIdAsync(3);

			// assert
			Assert.Null(result);
		}

		[Fact, Trait("Category", "Integration")]
		[TestPriority(10)]
		public async Task GetByIdAsync_WhenRecordExists_ReturnsRecord()
		{
			if (!IntegrationTestHelper.IsMartenDbRunning)
				return;

			// arrange

			// act
			var result = await _fixture.SubjectUnderTest.GetByIdAsync(1);

			// assert
			Assert.NotNull(result);
			Assert.Equal("a", result.FirstName);
		}

		[Fact, Trait("Category", "Integration")]
		[TestPriority(10)]
		public async Task GetByIdAsync_WhenCancellationRequestedIsTrue_ThrowsOperationCanceledException()
		{
			if (!IntegrationTestHelper.IsMartenDbRunning)
				return;

			// arrange
			var cancellationToken = new CancellationToken(true);

			// act
			var exception = await Record.ExceptionAsync(() => _fixture.SubjectUnderTest.GetByIdAsync(3, cancellationToken));

			// assert			
			Assert.NotNull(exception);
			Assert.IsType<OperationCanceledException>(exception);
			Assert.Equal("The operation was canceled.", exception.Message);
		}

		[Fact, Trait("Category", "Integration")]
		[TestPriority(10)]
		public async Task GetAllAsync_WhenRecordsExist_ReturnsAllTheRecords()
		{
			if (!IntegrationTestHelper.IsMartenDbRunning)
				return;

			// arrange

			// act
			var result = await _fixture.SubjectUnderTest.GetAllAsync();

			// assert
			Assert.True(result.Count() > 3);
		}

		[Fact, Trait("Category", "Integration")]
		[TestPriority(10)]
		public async Task GetAllAsync_WhenCancellationRequestedIsTrue_ThrowsOperationCanceledException()
		{
			if (!IntegrationTestHelper.IsMartenDbRunning)
				return;

			// arrange
			var cancellationToken = new CancellationToken(true);

			// act
			var exception = await Record.ExceptionAsync(() => _fixture.SubjectUnderTest.GetAllAsync(cancellationToken));

			// assert			
			Assert.NotNull(exception);
			Assert.IsType<OperationCanceledException>(exception);
			Assert.Equal("The operation was canceled.", exception.Message);
		}

		[Fact, Trait("Category", "Integration")]
		[TestPriority(10)]
		public async Task FindByWithoutIncludes_WhenRecordsExist_ReturnsRecordsThatMatchesTheFindByCriteria()
		{
			if (!IntegrationTestHelper.IsMartenDbRunning)
				return;

			// arrange

			// act
			var result = (await _fixture.SubjectUnderTest.FindByAsync(f => f.Id == 2)).FirstOrDefault();

			// assert
			Assert.NotNull(result);
			Assert.Equal("x", result.FirstName);
		}

		[Fact, Trait("Category", "Integration")]
		[TestPriority(20)]
		public async Task AddAsync_AddNewEntity_ReturnsAddedEntity()
		{
			if (!IntegrationTestHelper.IsMartenDbRunning)
				return;

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
		[TestPriority(20)]
		public async Task AddAsync_AddNewEntities_ReturnsAddedEntities()
		{
			if (!IntegrationTestHelper.IsMartenDbRunning)
				return;

			// arrange
			var student1 = new Fakes.Student
			{
				Id = 7,
				FirstName = "a",
				LastName = "b"
			};
			var student2 = new Fakes.Student
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
			Assert.Equal(expectedResult.Where(s => studentIds.Contains(s.Id)).Count(), actualResult.Count());
		}

		[Fact, Trait("Category", "Integration")]
		[TestPriority(20)]
		public async Task StoreAsync_AddNewEntities_ReturnsAddedEntities()
		{
			if (!IntegrationTestHelper.IsMartenDbRunning)
				return;

			var cancellationToken = new CancellationToken(false);
			var userRepository = _fixture.ServiceProvider.GetService<IRepository<User>>();

			// arrange
			var student = new Student
			{
				Id = 9,
				FirstName = "a",
				LastName = "b"
			};

			var user = new User
			{
				Id = 10,
				FirstName = "c",
				LastName = "d"
			};

			// act
			await _fixture.SubjectUnderTest.StoreAsync(cancellationToken, student, user);
			var expectedStudent = await _fixture.SubjectUnderTest.GetByIdAsync(9);
			var expectedUser = await userRepository.GetByIdAsync(10);

			// assert
			Assert.NotNull(expectedStudent);
			Assert.NotNull(expectedUser);
		}

		[Fact, Trait("Category", "Integration")]
		[TestPriority(20)]
		public async Task AddAsync_WhenCancellationRequestedIsTrue_ThrowsOperationCanceledException()
		{
			if (!IntegrationTestHelper.IsMartenDbRunning)
				return;

			// arrange
			var cancellationToken = new CancellationToken(true);
			const int newStudentId = 9;
			var student = new Student
			{
				Id = newStudentId,
				FirstName = "cc",
				LastName = "dd"
			};

			// act
			var exception = await Record.ExceptionAsync(() => _fixture.SubjectUnderTest.AddAsync(student, cancellationToken));

			// assert			
			Assert.NotNull(exception);
			Assert.IsType<OperationCanceledException>(exception);
			Assert.Equal("The operation was canceled.", exception.Message);
		}


		[Fact, Trait("Category", "Integration")]
		[TestPriority(30)]
		public async Task UpdateAsync_UpdateAnExistingEntity_UpdatesTheEntity()
		{
			if (!IntegrationTestHelper.IsMartenDbRunning)
				return;

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
		[TestPriority(30)]
		public async Task UpdateAsync_WhenCancellationRequestedIsTrue_ThrowsOperationCanceledException()
		{
			if (!IntegrationTestHelper.IsMartenDbRunning)
				return;

			// arrange
			var student = await _fixture.SubjectUnderTest.GetByIdAsync(1);
			var cancellationToken = new CancellationToken(true);
			student.FirstName = "cc";

			// act
			var exception = await Record.ExceptionAsync(() => _fixture.SubjectUnderTest.UpdateAsync(student, cancellationToken));

			// assert			
			Assert.NotNull(exception);
			Assert.IsType<OperationCanceledException>(exception);
			Assert.Equal("The operation was canceled.", exception.Message);
		}

		[Fact, Trait("Category", "Integration")]
		[TestPriority(40)]
		public async Task DeleteAsync_DeleteAfterFetching_DeletesTheEntity()
		{
			if (!IntegrationTestHelper.IsMartenDbRunning)
				return;

			// arrange
			var student = await _fixture.SubjectUnderTest.GetByIdAsync(10);

			// act
			await _fixture.SubjectUnderTest.DeleteAsync(10);

			// assert
			var queryResult = await _fixture.SubjectUnderTest.GetByIdAsync(10);
			Assert.Null(queryResult);
		}

		[Fact, Trait("Category", "Integration")]
		[TestPriority(40)]
		public async Task DeleteAsync_DeleteById_DeletesTheEntity()
		{
			if (!IntegrationTestHelper.IsMartenDbRunning)
				return;

			if (!IntegrationTestHelper.IsMartenDbRunning)
				return;

			// arrange

			// act
			await _fixture.SubjectUnderTest.DeleteAsync(11);
			var queryResult = await _fixture.SubjectUnderTest.GetByIdAsync(11);

			// assert
			Assert.Null(queryResult);
		}

		[Fact, Trait("Category", "Integration")]
		[TestPriority(40)]
		public async Task DeleteAsync_WhenCancellationRequestedIsTrue_ThrowsOperationCanceledException()
		{
			if (!IntegrationTestHelper.IsMartenDbRunning)
				return;

			// arrange
			var student = await _fixture.SubjectUnderTest.GetByIdAsync(10);
			var cancellationToken = new CancellationToken(true);

			// act
			var exception = await Record.ExceptionAsync(() => _fixture.SubjectUnderTest.DeleteAsync(10, cancellationToken));

			// assert			
			Assert.NotNull(exception);
			Assert.IsType<OperationCanceledException>(exception);
			Assert.Equal("The operation was canceled.", exception.Message);
		}
	}
}
