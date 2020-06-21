Entity
------
You could create an entity by inheriting `BaseEntity<TIdType>` where TIdType is the type of the Id property.

IRepository
-----------
[`IRepository`](https://github.com/gokulm/BoltOn/blob/master/src/BoltOn/Data/IRepository.cs) interface in the core BoltOn package is abstracted out to keep it agnostic of any particular database, so that the implementation could be changed easily while bootstrapping the application when we switch databases (which rarely happens though). 

The disadvantage of this abstraction is, we lose certain native features of the underlying database specific features, as it isn't easy to come up with a common IRepository interface to support all the databases. So, please feel free to override the existing implementation or come up with your own implemenation for some of the missing methods.

Entity Framework Repository
---------------------------
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
* The mapping classes will be automatically added to your DbContext if you inherit `BaseDbContext<TDbContext>` and if they are in the same assembly where the DbContext resides.
* In case if you do not want all the mapping configuration classes in the assembly to be added, you can override `ApplyConfigurations(ModelBuilder modelBuilder)` method of `BaseDbContext` and add the configuration classes manually.

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

and register like this:

	serviceCollection.AddTransient<IRepository<Student>, Repository<Student, SchoolDbContext>>();

If you want to override the Repository class' methods or if you want to add methods, create your own repository, like this:

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

DbContextFactory
----------------
This factory uses IServiceProvider to resolve DbContexts, and if the request implements `IQuery<>`, it sets the DbContexts' `ChangeTracker.QueryTrackingBehavior` to `QueryTrackingBehavior.NoTracking` and `ChangeTracker.AutoDetectChangesEnabled` to false with the help of `ChangeTrackerInterceptor`. 

**Note:** You could disable this behavior by removing the interceptor from the pipeline using `RemoveInterceptor<ChangeTrackerInterceptor>` extension method.

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

and register like this:

	serviceCollection.AddTransient<IRepository<Student>, Repository<Student, SchoolCosmosDbOptions>>();

If you want to override the Repository class' methods or if you want to add methods, create your own repository, like this:

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

* While using any property in CosmosDb query, make sure property name matches exactly as it is in stored in the document collection.
* Since EF 3.0+ supports CosmosDb, feel free to use EF directly instead of BoltOn.Data.CosmosDb

Elasticsearch
-------------
BoltOn.Data.Elasticsearch uses [NEST](https://www.nuget.org/packages/NEST/) library internally.

In order to use Elasticsearch, you need do the following:

* Install **BoltOn.Data.Elasticsearch** NuGet package.
* Call `BoltOnElasticsearchModule()` in your startup's BoltOn() method.
* Create an entity by inheriting `BaseEntity<TIdType>`. The inheritance is not mandatory though.
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
    public class Student : BaseEntity<int>
	{
		public string FirstName { get; set; }
		public string LastName { get; set; }
	}

and register like this:

	serviceCollection.AddTransient<IRepository<Student>, Repository<Student, SchoolElasticDbOptions>>()

**Note:** The existing FindByAsync doesn't support find by expression, so pass null for the predicate param and NEST's `SearchRequest` for the options param.  

Example:

	var searchRequest = new SearchRequest
	{
		Query = new MatchQuery { Field = "firstName", Query = "John" }
	};

	repository.FindByAsync(null, searchRequest)
