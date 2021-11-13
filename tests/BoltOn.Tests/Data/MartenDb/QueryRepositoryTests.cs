using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BoltOn.Tests.Data.MartenDb.Fakes;
using BoltOn.Tests.Other;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BoltOn.Tests.Data.MartenDb
{
	[Collection("IntegrationTests")]
	public class QueryRepositoryTests : IClassFixture<MartenDbQueryRepositoryFixture>
	{
		private readonly MartenDbQueryRepositoryFixture _fixture;

		public QueryRepositoryTests(MartenDbQueryRepositoryFixture fixture)
		{
			_fixture = fixture;
		}

		[Fact, Trait("Category", "Integration")]
		public async Task GetById_WhenRecordExists_ReturnsRecord()
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
		public async Task GetById_WhenRecordDoesNotExist_ReturnsNull()
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
		public async Task GetByIdAsync_WhenCancellationRequestedIsTrue_ThrowsTaskCanceledException()
		{
			if (!IntegrationTestHelper.IsMartenDbRunning)
				return;

			// arrange
			var cancellationToken = new CancellationToken(true);

			// act
			var exception = await Record.ExceptionAsync(() => _fixture.SubjectUnderTest.GetByIdAsync(1, cancellationToken));

			// assert			
			Assert.NotNull(exception);
			Assert.IsType<TaskCanceledException>(exception);
			Assert.Equal("A task was canceled.", exception.Message);
		}

		[Fact, Trait("Category", "Integration")]
		public async Task GetAll_WhenRecordsExist_ReturnsAllTheRecords()
		{
			if (!IntegrationTestHelper.IsMartenDbRunning)
				return;

			// arrange

			// act
			var result = (await _fixture.SubjectUnderTest.GetAllAsync()).ToList();

			// assert
			Assert.Equal(4, result.Count);
		}

		[Fact, Trait("Category", "Integration")]
		public async Task GetAllAsync_WhenRecordsExist_ReturnsAllTheRecords()
		{
			if (!IntegrationTestHelper.IsMartenDbRunning)
				return;

			// arrange

			// act
			var result = await _fixture.SubjectUnderTest.GetAllAsync();

			// assert
			Assert.Equal(4, result.Count());
		}

		[Fact, Trait("Category", "Integration")]
		public async Task GetAllAsync_WhenCancellationRequestedIsTrue_ThrowsTaskCanceledException()
		{
			if (!IntegrationTestHelper.IsMartenDbRunning)
				return;

			// arrange
			var cancellationToken = new CancellationToken(true);

			// act
			var exception = await Record.ExceptionAsync(() => _fixture.SubjectUnderTest.GetAllAsync(cancellationToken));

			// assert			
			Assert.NotNull(exception);
			Assert.IsType<TaskCanceledException>(exception);
			Assert.Equal("A task was canceled.", exception.Message);
		}

		[Fact, Trait("Category", "Integration")]
		public async Task FindByWithoutIncludes_WhenRecordsExist_ReturnsRecordsThatMatchesTheFindByCriteria()
		{
			if (!IntegrationTestHelper.IsMartenDbRunning)
				return;

			// arrange
			var schoolDbContext = _fixture.ServiceProvider.GetService<SchoolDbContext>();

			// act
			var result = (await _fixture.SubjectUnderTest.FindByAsync(f => f.Id == 2)).FirstOrDefault();

			// assert
			Assert.NotNull(result);
			Assert.Equal("x", result.FirstName);
		}
	}
}
