using System;
using BoltOn.Data;
using BoltOn.Data.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using BoltOn.Bootstrapping;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace BoltOn.Tests.Data
{
	public class EFRepositoryTests : IDisposable
	{
		private ISchoolRepository _sut;

		[Fact]
		public void GetById_WhenRecordExists_ReturnsRecord()
		{
			// arrange
			var serviceCollection = new ServiceCollection();
			var serviceProvider = SetUpInMemoryDb(serviceCollection);

			// act
			var result = _sut.GetById<Student>(1);

			// assert
			Assert.NotNull(result);
			Assert.Equal("a", result.FirstName);
		}

		[Fact]
		public void GetById_WhenRecordDoesNotExist_ReturnsNull()
		{
			// arrange
			var serviceCollection = new ServiceCollection();
			var serviceProvider = SetUpInMemoryDb(serviceCollection);

			// act
			var result = _sut.GetById<Student>(3);

			// assert
			Assert.Null(result);
		}

		[Fact]
		public async Task GetByIdAsync_WhenRecordExists_ReturnsRecord()
		{
			// arrange
			var serviceCollection = new ServiceCollection();
			var serviceProvider = SetUpInMemoryDb(serviceCollection);

			// act
			var result = await _sut.GetByIdAsync<Student>(1);

			// assert
			Assert.NotNull(result);
			Assert.Equal("a", result.FirstName);
		}

		[Fact]
		public void GetAll_WhenRecordsExist_ReturnsAllTheRecords()
		{
			// arrange
			var serviceCollection = new ServiceCollection();
			var serviceProvider = SetUpInMemoryDb(serviceCollection);

			// act
			var result = _sut.GetAll<Student>().ToList();

			// assert
			Assert.Equal(2, result.Count);
		}

		[Fact]
		public async Task GetAllAsync_WhenRecordsExist_ReturnsAllTheRecords()
		{
			// arrange
			var serviceCollection = new ServiceCollection();
			var serviceProvider = SetUpInMemoryDb(serviceCollection);

			// act
			var result = await _sut.GetAllAsync<Student>();

			// assert
			Assert.Equal(2, result.Count());
		}

		[Fact]
		public void FindBy_WhenRecordsExist_ReturnsRecordsThatMatchesTheCriteria()
		{
			// arrange
			var serviceCollection = new ServiceCollection();
			var serviceProvider = SetUpInMemoryDb(serviceCollection);

			// act
			var result = _sut.FindBy<Student>(f => f.Id == 2).FirstOrDefault();

			// assert
			Assert.NotNull(result);
			Assert.Equal("x", result.FirstName);
			Assert.Empty(result.Courses);
		}



		private ServiceProvider SetUpInMemoryDb(IServiceCollection serviceCollection)
		{
			serviceCollection.AddDbContext<SchoolDbContext>(options => options.UseInMemoryDatabase("InMemoryDbForTesting"));
			serviceCollection.BoltOn();
			var serviceProvider = serviceCollection.BuildServiceProvider();
			var testDbContext = serviceProvider.GetService<SchoolDbContext>();
			testDbContext.Set<Student>().Add(new Student
			{
				Id = 1,
				FirstName = "a",
				LastName = "b"
			});
			testDbContext.Set<Student>().Add(new Student
			{
				Id = 2,
				FirstName = "x",
				LastName = "y",
				Courses = new List<Course>
				{
					new Course { Id = Guid.NewGuid(), Name = "Computer Science"}
				}
			});
			testDbContext.SaveChanges();
			serviceProvider.UseBoltOn();
			_sut = serviceProvider.GetService<ISchoolRepository>();
			return serviceProvider;
		}

		public void Dispose()
		{
			Bootstrapper
				.Instance
				.Dispose();
		}
	}

	public interface ISchoolRepository : IRepository
	{

	}

	public class SchoolRepository : BaseEFRepository<SchoolDbContext>, ISchoolRepository
	{
		public SchoolRepository(IDbContextFactory dbContextFactory) : base(dbContextFactory)
		{
		}
	}

	public class SchoolDbContext : BaseDbContext<SchoolDbContext>
	{
		public SchoolDbContext(DbContextOptions<SchoolDbContext> options) : base(options)
		{
		}
	}

	public class Student : BaseEntity<int>
	{
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public IEnumerable<Course> Courses { get; set; }
	}

	public class Course : BaseEntity<Guid>
	{
		public string Name { get; set; }
	}

	public class StudentMapping : IEntityTypeConfiguration<Student>
	{
		public void Configure(EntityTypeBuilder<Student> builder)
		{
			builder
				.ToTable("Student")
				.HasKey(k => k.Id);
			builder
				.Property(p => p.Id)
				.HasColumnName("StudentId");
			builder.
				HasMany(p => p.Courses);
		}
	}

	public class CourseMapping : IEntityTypeConfiguration<Course>
	{
		public void Configure(EntityTypeBuilder<Course> builder)
		{
			builder
				.ToTable("Course")
				.HasKey(k => k.Id);
			builder
				.Property(p => p.Id)
				.HasColumnName("CourseId");
		}
	}
}
