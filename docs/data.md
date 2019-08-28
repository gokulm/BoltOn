Entity
------
You could create an entity by inheriting `BaseEntity<TIdType>` where TIdType is the type of the Id property.

Entity Framework Repository
---------------------------
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


DbContextFactory
----------------
This factory uses IServiceProvider to resolve DbContexts, and if the request implements `IQuery<>`, it sets the DbContexts' `ChangeTracker.QueryTrackingBehavior` to `QueryTrackingBehavior.NoTracking` and `ChangeTracker.AutoDetectChangesEnabled` to false with the help of `MediatorContextInterceptor`. 

**Note:** You could disable this behavior by removing the interceptor from the pipeline using `RemoveInterceptor<TInterceptor>` extension method.

CosmosDb
--------
`BaseCosmosDbRepository<TEntity, TCosmosDbContext>` implements `IRepository<TEntity>`, so any Entity Framework SQL repository can be used against CosmosDb. 

In order to use CosmosDb, you need do the following:

1. Install **BoltOn.Data.CosmosDb** NuGet package.
2. Call `BoltOnCosmosDbModule()` in your startup's BoltOn() method.
3. Use AddCosmosDbContext extension method to initialize options like URI, AuthorizationKey and Database Name.
4. Create an entity by inheriting `BaseEntity<TIdType>`. The inheritance is not mandatory though.
5. Create a DbContext by inheriting `BaseCosmosDbContext<TDbContext>`. 
6. Inherit `BaseCosmosDbRepository<TEntity, TCosmosDbContext>`.

Example:

	services.BoltOn(options =>
	{
		options.BoltOnCosmosDbModule();
	});

	services.AddCosmosDbContext<CollegeDbContext>(options =>
	{
		options.Uri = "<<SPECIFY URI>>";
		options.AuthorizationKey = "<<SPECIFY AUTHORIZATION KEY>>";
		options.DatabaseName = "<<DATABASE NAME>>";
	});

	// DbContext
	public class CollegeDbContext : BaseCosmosDbContext<CollegeDbContext>
    {
        public CollegeDbContext(CosmosDbContextOptions<CollegeDbContext> options) : base(options)
        {
        }
    }

	// Entity
	public class Grade : BaseEntity<string>
    {
        [JsonProperty("id")]
        public override string Id { get; set; }
        [JsonProperty("studentId")]
        public int StudentId { get; set; }
        public string CourseName { get; set; }
        public int Year { get; set; }
        public string Score { get; set; }
    }

	// Repository
	public interface IGradeRepository : IRepository<Grade>
    {
		// CosmosDb specific query methods can be added
        Task<Grade> GetByIdAsync<TId>(TId id, object partitionKey);
    }

	public class GradeRepository : BaseCosmosDbRepository<Grade, CollegeDbContext>, IGradeRepository
    {
        public GradeRepository(CollegeDbContext collegeDbContext) : base(collegeDbContext)
        {
        }

        public virtual async Task<Grade> GetByIdAsync<TId>(TId id, object partitionKey)
        {
            var document = await DocumentClient.ReadDocumentAsync<Grade>(GetDocumentUri(id.ToString()), new RequestOptions { PartitionKey = new PartitionKey(partitionKey) });
            return document.Document;
        }
    }

**Note:** While using any property in CosmosDb query, make sure propertyname matches exactly as it is in stored in the document collection.
