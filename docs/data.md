Entity
------
You could create an entity by inheriting `BaseEntity<TIdType>` where TIdType is the type of the Id property.

Entity Framework Repository
---------------------------
The core BoltOn package has only the [`IRepository`](https://github.com/gokulm/BoltOn/blob/master/src/BoltOn/Data/IRepository.cs) interface, which could be used in your domain layer if you're into Domain Driven Design. 

In order to use Entity Framework implementation of the repository, you need to do the following:

* Install **BoltOn.Data.EF** NuGet package.
* Call `BoltOnEFModule()` in your startup's BoltOn() method.
* Create an entity by inheriting `BaseEntity<TIdType>`. The inheritance is not mandatory though.
* Create a DbContext by inheriting [`BaseDbContext<TDbContext>`](https://github.com/gokulm/BoltOn/blob/master/src/BoltOn.Data.EF/BaseDbContext.cs). You could inherit EF's DbContext directy if you're not interested in any of the benefits that BaseDbContext offers.
* Inherit [`Repository<TEntity, TDbContext>`](https://github.com/gokulm/BoltOn/blob/master/src/BoltOn.Data.EF/Repository.cs) to create a repository for your entity.
* All the repository methods accept an optional parameter options, in FindBy methods, if other navigation properties need to be fetched, a collection of expressions can be passed.

Example:

	var includes = new List<Expression<Func<Student, object>>>
	{
		s => s.Addresses
	};

	// act
	var result = repository.FindBy(f => f.Id == 2, includes).FirstOrDefault();

* Add all the database columns to entity properties mapping inside a mapping class by implementing `IEntityTypeConfiguration<TEntity>` interface.
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

	public class StudentRepository : Repository<Student, SchoolDbContext>, IStudentRepository
	{
		public StudentRepository(IDbContextFactory dbContextFactory, EventBag eventBag, IBoltOnClock boltOnClock)
			: base(dbContextFactory, eventBag, boltOnClock)
		{
		}
	}

**Note:** Create your own repository like StudentRepository only if you want to override the Repository class' methods or if you want to add methods, else just register `IRepository<Student>` to `Repository<Student, SchoolDbContext>`

DbContextFactory
----------------
This factory uses IServiceProvider to resolve DbContexts, and if the request implements `IQuery<>`, it sets the DbContexts' `ChangeTracker.QueryTrackingBehavior` to `QueryTrackingBehavior.NoTracking` and `ChangeTracker.AutoDetectChangesEnabled` to false with the help of `MediatorContextInterceptor`. 

**Note:** You could disable this behavior by removing the interceptor from the pipeline using `RemoveInterceptor<TInterceptor>` extension method.

CosmosDb
--------
In order to use CosmosDb, you need do the following:

* Install **BoltOn.Data.CosmosDb** NuGet package.
* Call `BoltOnCosmosDbModule()` in your startup's BoltOn() method.
* Create an entity by inheriting `BaseEntity<TIdType>`. The inheritance is not mandatory though.
* Create an options class by inheriting `BaseCosmosDbOptions` class. 
* Use AddCosmosDb extension method to initialize options like URI, AuthorizationKey and Database Name.
* Inherit `Repository<TEntity, TCosmosDbOptions>` to create a repository for your entity.
* All the repository methods accept an optional parameter options. For some of the methods, RequestOptions can be passed and for some FeedOptions can be passed as the options object, take a look at the [`Repository<TEntity, TCosmosDbOptions>`](https://github.com/gokulm/BoltOn/blob/master/src/BoltOn.Data.CosmosDb/Repository.cs) to see the implementation.

Example:

	services.BoltOn(options =>
	{
		options.BoltOnCosmosDbModule();
	});

	services.AddCosmosDb<SchoolCosmosDbOptions>(options =>
	{
		options.Uri = "<<SPECIFY URI>>";
		options.AuthorizationKey = "<<SPECIFY AUTHORIZATION KEY>>";
		options.DatabaseName = "<<DATABASE NAME>>";
	});

	public class SchoolCosmosDbOptions : BaseCosmosDbOptions
    {
    }

	// Entity
	public class Student : BaseEntity<string>
    {
        [JsonProperty("id")]
        public override string Id { get; set; }
        [JsonProperty("studentId")]
        public int StudentId { get; set; }
        public string FirstName { get; set; }
    }

	// Repository
    public interface IStudentRepository : IRepository<Student>
	{
	}

	public class StudentRepository : Repository<Student, SchoolCosmosDbOptions>, IStudentRepository
	{
		public StudentRepository(SchoolCosmosDbOptions options, EventBag eventBag, IBoltOnClock boltOnClock)
			: base(options, eventBag, boltOnClock)
		{
		}
	}

**Note:** 

* Create your own repository like StudentRepository only if you want to override the Repository class' methods or if you want to add methods, else just register `IRepository<Student>` to `Repository<Student, SchoolCosmosDbOptions>`
* While using any property in CosmosDb query, make sure property name matches exactly as it is in stored in the document collection.
