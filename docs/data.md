Entity
------
You could create an entity by inheriting `BaseEntity<TIdType>` where TIdType is the type of the Id property.

Repository
----------
The core BoltOn package has only the `IRepository` interface, which could be used in your domain layer if you're into Domain Driven Design. 

In order to use Entity Framework implementation of the repository, you need to do the following:

1. Install **BoltOn.Data.EF** NuGet package.
2. Call `BoltOnEFModule()` in your startup's BoltOn() method.
3. Create an entity by inheriting `BaseEntity<TIdType>`. The inheritance is not mandatory though.
4. Create a DbContext by inheriting `BaseDbContext<TDbContext>`. You could inherit EF's DbContext directy if you're not interested in any of the benefits that BaseDbContext offers.
5. Inherit `BaseEFRepository<TEntity, TDbContext>`.
6. Add all the database columns to entity properties mapping inside a mapping class by implementing `IEntityTypeConfiguration<TEntity>` interface.
<br>
The mapping classes will be automatically added to your DbContext if you inherit `BaseDbContext<TDbContext>` and if they are in the same assembly where the DbContext resides. 

Example:

    // DbContext
    public class SchoolDbContext : BaseDbContext<SchoolDbContext>
	{
		public SchoolDbContext(DbContextOptions<SchoolDbContext> options) : base(options)
		{
		}
	}

    // Entity
    public class Student : BaseEntity<int>
	{
		public string FirstName { get; set; }
		public string LastName { get; set; }
	}

    // Mapping
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
			builder
				.HasMany(p => p.Addresses)
				.WithOne(p => p.Student);
		}
	}

    // Repository
    public interface IStudentRepository : IRepository<Student>
	{
	}

	public class StudentRepository : BaseEFRepository<Student, SchoolDbContext>, IStudentRepository
	{
		public StudentRepository(IDbContextFactory dbContextFactory) : base(dbContextFactory)
		{
		}
	}

**Note: ** You could create just one repository for the entire database and use it for all the entities like `ISchoolRepository<Student>`, `ISchoolRepository<Address>` etc., if the methods in the `BaseEFRepository<>` are good enough.

BoltOn.Mediator.Data.EF Module
------------------------------
 In case if you want to set your DbContexts' ChangeTracker.QueryTrackingBehavior to QueryTrackingBehavior.NoTracking and disable ChangeTracker.AutoDetectChangesEnabled, install **BoltOn.Mediator.Data.EF** module and call `BoltOnMediatorEFModule()` in your startup's BoltOn() method 


