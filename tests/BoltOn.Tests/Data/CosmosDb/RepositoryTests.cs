using System;
using BoltOn.Data.EF;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using BoltOn.Bootstrapping;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;
using BoltOn.Tests.Other;

namespace BoltOn.Tests.Data.CosmosDb
{
    // Collection is used to prevent running tests in parallel i.e., tests in the same collection
    // will not be executed in parallel
    [Collection("IntegrationTests")]
    public class RepositoryTests : IDisposable
    {
        private readonly IGradeRepository _sut;

        public RepositoryTests()
        {
            // this flag can be set to true for [few] tests. Running all the tests with this set to true might slow down.
            MediatorTestHelper.IsSqlServer = false;
            var serviceCollection = new ServiceCollection();
            serviceCollection
                .BoltOn(options =>
                {
                    options
                        .BoltOnEFModule();
                });
            var serviceProvider = serviceCollection.BuildServiceProvider();
            serviceProvider.TightenBolts();
            MediatorTestHelper.IsSeedData = true;
            _sut = serviceProvider.GetService<IGradeRepository>();
        }

        [Fact, Trait("Category", "Integration")]
        public void GetById_WhenRecordExists_ReturnsRecord()
        {
            // arrange
            var id = Guid.NewGuid().ToString();
            var grade = new Grade { Id = id, StudentId = 1, CourseName = "physics", Score = "A-", Year = 2017 };
            _sut.Add(grade);

            // act
            var result = _sut.GetById(id);

            // assert
            Assert.NotNull(result);
            Assert.Equal("A-", result.Score);
        }

        [Fact, Trait("Category", "Integration")]
        public void GetById_WhenRecordDoesNotExist_ReturnsNull()
        {
            // arrange

            // act
            var result = _sut.GetById(Guid.NewGuid());

            // assert
            Assert.Null(result);
        }

        [Fact, Trait("Category", "Integration")]
        public async Task GetByIdAsync_WhenRecordExists_ReturnsRecord()
        {
            // arrange
            var id = Guid.NewGuid().ToString();
            var grade = new Grade { Id = id, StudentId = 1, CourseName = "physics", Score = "A-", Year = 2017 };
            await _sut.AddAsync(grade);

            // act
            var result = await _sut.GetByIdAsync(id);

            // assert
            Assert.NotNull(result);
            Assert.Equal(2017, result.Year);
        }

        [Fact, Trait("Category", "Integration")]
        public void GetAll_WhenRecordsExist_ReturnsAllTheRecords()
        {
            // arrange
            var id1 = Guid.NewGuid().ToString();
            var grade1 = new Grade { Id = id1, StudentId = 1, CourseName = "physics", Score = "A-", Year = 2017 };
            _sut.Add(grade1);
            var id2 = Guid.NewGuid().ToString();
            var grade2 = new Grade { Id = id2, StudentId = 1, CourseName = "physics", Score = "A-", Year = 2017 };
            _sut.Add(grade2);
            

            // act
            var result = _sut.GetAll().ToList();

            // assert
            Assert.Equal(2, result.Count);
        }

        [Fact, Trait("Category", "Integration")]
        public async Task GetAllAsync_WhenRecordsExist_ReturnsAllTheRecords()
        {
            // arrange
            var id1 = Guid.NewGuid().ToString();
            var grade1 = new Grade { Id = id1, StudentId = 1, CourseName = "physics", Score = "A-", Year = 2017 };
            await _sut.AddAsync(grade1);
            var id2 = Guid.NewGuid().ToString();
            var grade2 = new Grade { Id = id2, StudentId = 1, CourseName = "physics", Score = "A-", Year = 2017 };
            await _sut.AddAsync(grade2);

            // act
            var result = await _sut.GetAllAsync();

            // assert
            Assert.Equal(2, result.Count());
        }

        [Fact, Trait("Category", "Integration")]
        public void FindBy_WhenRecordsExist_ReturnsRecordsThatMatchesTheFindByCriteria()
        {
            // arrange
            var id = Guid.NewGuid().ToString();
            var grade = new Grade { Id = id, StudentId = 1, CourseName = "physics", Score = "A-", Year = 2017 };
            _sut.Add(grade);

            // act
            var result = _sut.FindBy(f => f.Id == id).FirstOrDefault();

            // assert
            Assert.NotNull(result);
            Assert.Equal(1, result.StudentId);
        }


        [Fact, Trait("Category", "Integration")]
        public async Task FindByAsync_WhenRecordDoesNotExist_ReturnsNull()
        {
            // arrange

            // act
            var result = (await _sut.FindByAsync(f => f.StudentId == 2, default)).FirstOrDefault();

            // assert
            Assert.Null(result);
        }

        [Fact, Trait("Category", "Integration")]
        public void Add_AddANewEntity_ReturnsAddedEntity()
        {
            // arrange
            var id = Guid.NewGuid().ToString();
            var grade = new Grade { Id = id, StudentId = 1, CourseName = "physics", Score = "A-", Year = 2017 };
            
            // act
            var result = _sut.Add(grade);
            var queryResult = _sut.GetById(id);

            // assert
            Assert.NotNull(queryResult);
            Assert.Equal(1, queryResult.StudentId);
            Assert.Equal(result.CourseName, queryResult.CourseName);
        }

        [Fact, Trait("Category", "Integration")]
        public async Task AddAsync_AddANewEntity_ReturnsAddedEntity()
        {
            // arrange
            var id = Guid.NewGuid().ToString();
            var grade = new Grade { Id = id, StudentId = 1, CourseName = "physics", Score = "A-", Year = 2017 };

            // act
            var result = await _sut.AddAsync(grade);
            var queryResult = await _sut.GetByIdAsync(id);

            // assert
            Assert.NotNull(queryResult);
            Assert.Equal("A-", queryResult.Score);
        }

        [Fact, Trait("Category", "Integration")]
        public void Update_UpdateAnExistingEntity_UpdatesTheEntity()
        {
            // arrange
            var id = Guid.NewGuid().ToString();
            var grade = new Grade { Id = id, StudentId = 1, CourseName = "physics", Score = "A-", Year = 2017 };
            _sut.Add(grade);

            // act
            grade.Score = "A";
            _sut.Update(grade);
            var queryResult = _sut.GetById(id);

            // assert
            Assert.NotNull(queryResult);
            Assert.Equal("A", queryResult.Score);
        }

        [Fact, Trait("Category", "Integration")]
        public async Task UpdateAsync_UpdateAnExistingEntity_UpdatesTheEntity()
        {
            // arrange
            var id = Guid.NewGuid().ToString();
            var grade = new Grade { Id = id, StudentId = 1, CourseName = "physics", Score = "A-", Year = 2017 };
            await _sut.AddAsync(grade);

            // act
            grade.StudentId = 2;
            await _sut.UpdateAsync(grade);
            var queryResult = _sut.GetById(id);

            // assert
            Assert.NotNull(queryResult);
            Assert.Equal(2, queryResult.StudentId);
        }

        public void Dispose()
        {
            Bootstrapper
                .Instance
                .Dispose();
        }
    }
}
