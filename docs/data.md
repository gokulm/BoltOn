Repository 
-----------
[`IRepository`](https://github.com/gokulm/BoltOn/blob/master/src/BoltOn/Data/IRepository.cs) and [`IQueryRepository`](https://github.com/gokulm/BoltOn/blob/master/src/BoltOn/Data/IQueryRepository.cs) interfaces in the core BoltOn package is abstracted out to keep it agnostic of any particular database. However, they're currently implemented only in BoltOn.Data.EF package. 

There are separate `IRepository` interfaces in CosmosDb and ElasticSearch packages too.

Entity Framework Repository
---------------------------
In order to use Entity Framework implementation of the repository, you need to do the following:

* Install **BoltOn.Data.EF** NuGet package.
* Call `BoltOnEFModule()` in your startup's BoltOn() method.
* Register [`IRepository<TEntity>`](https://github.com/gokulm/BoltOn/blob/master/src/BoltOn/Data/IRepository.cs) to [`Repository<TEntity, TDbContext>`](https://github.com/gokulm/BoltOn/blob/master/src/BoltOn.Data.EF/Repository.cs) or [`IQueryRepository<TEntity>`](https://github.com/gokulm/BoltOn/blob/master/src/BoltOn/Data/IQueryRepository.cs) to [`QueryRepository<TEntity, TDbContext>`](https://github.com/gokulm/BoltOn/blob/master/src/BoltOn.Data.EF/QueryRepository.cs).
* `IQueryRepository` has only methods related to data retrieval, and it sets EF's `QueryTrackingBehavior` to `QueryTrackingBehavior.NoTracking`.

Example:

	services.BoltOn(options =>
	{
		options.BoltOnEFModule();
	});

	services.AddDbContext<SchoolDbContext>(options =>
	{
		options.UseSqlServer("connectionstring");
	});
	
	services.AddTransient<IRepository<Student>, Repository<Student, SchoolDbContext>>();
	services.AddTransient<IQueryRepository<Course>, QueryRepository<Student, SchoolDbContext>>();

* The database table mappings can be added within your DbContext or in a separate class by implementing `IEntityTypeConfiguration<TEntity>` interface.

Example:

    // DbContext
    public class SchoolDbContext : DbContext
	{
		public SchoolDbContext(DbContextOptions<SchoolDbContext> options) : base(options)
		{
		}
	}

    // Entity
    public class Student 
	{
		public int StudentId { get; set; }
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
				.HasKey(k => k.StudentId);
			builder
				.Property(p => p.Id)
				.HasColumnName("StudentId");
			builder
				.HasMany(p => p.Addresses)
				.WithOne(p => p.Student);
		}
	}

* You can override the Repository class' methods or add methods by inheriting `Repository` or `QueryRepository`.

Example:

    // Repository
    public interface IStudentRepository : IRepository<Student>
	{
	}

	public class StudentRepository : Repository<Student, SchoolDbContext>, IStudentRepository
	{
		public StudentRepository(SchoolDbContext schoolDbContext)
			: base(schoolDbContext)
		{
		}
	}

** Note: ** Entity Framework has an extension method `ApplyConfigurationsFromAssembly` to add all the configurations in an assembly; in case if you want to add configurations only from certain namespace in an assembly, the extension method `ApplyConfigurationsFromNamespaceOfType<T>`, which is part of **BoltOn.Data.EF** can be used.

Like this:

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);
		modelBuilder.ApplyConfigurationsFromNamespaceOfType<StudentMapping>();
	}

CosmosDb
--------
In order to use CosmosDb, you need do the following:

* Install **BoltOn.Data.CosmosDb** NuGet package.
* Call `BoltOnCosmosDbModule()` in your startup's BoltOn() method.
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
	public class Student 
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("studentId")]
        public int StudentId { get; set; }
        public string FirstName { get; set; }
    }

and register like this:

	serviceCollection.AddTransient<IRepository<Student>, Repository<Student, SchoolCosmosDbOptions>>();

If you want to override the Repository class' methods or if you want to add methods, create your own repository, like this:

	// Repository
    public interface IStudentRepository : IRepository<Student>
	{
	}

	public class StudentRepository : Repository<Student, SchoolCosmosDbOptions>, IStudentRepository
	{
		public StudentRepository(SchoolCosmosDbOptions options)
			: base(options)
		{
		}
	}

**Note:** 

* While using any property in CosmosDb query, make sure property name matches exactly as it is in stored in the document collection.
* Since EF 3.0+ supports CosmosDb, feel free to use EF directly instead of BoltOn.Data.CosmosDb

Elasticsearch
-------------
BoltOn.Data.Elasticsearch uses [NEST](https://www.nuget.org/packages/NEST/) library internally.

In order to use Elasticsearch, you need do the following:

* Install **BoltOn.Data.Elasticsearch** NuGet package.
* Call `BoltOnElasticsearchModule()` in your startup's BoltOn() method.
* Create an options class by inheriting `BaseElasticsearchOptions` class. 
* Use AddElasticsearch extension method to initialize NEST library's ConnectionSettings.
* Inherit `Repository<TEntity, TElasticsearchOptions>` to create a repository for your entity.
* Here is the implementation [`Repository<TEntity, TElasticsearchOptions>`](https://github.com/gokulm/BoltOn/blob/master/src/BoltOn.Data.Elasticsearch/Repository.cs).

Example:

	services.BoltOn(options =>
	{
		options.BoltOnElasticsearchModule();
	});

	services.AddElasticsearch<SchoolElasticDbOptions>(options =>
	{
		options.ConnectionSettings = new Nest.ConnectionSettings(new Uri("http://127.0.0.1:9200"));
	});

	public class SchoolElasticDbOptions : BaseElasticsearchOptions
    {
    }

	// Entity
    public class Student 
	{
		public int StudentId { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
	}

and register like this:

	serviceCollection.AddTransient<IRepository<Student>, Repository<Student, SchoolElasticDbOptions>>()

Example:

	var searchRequest = new SearchRequest
	{
		Query = new MatchQuery { Field = "firstName", Query = "John" }
	};

	repository.FindByAsync(searchRequest)
