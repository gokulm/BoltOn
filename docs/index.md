[**BoltOn**](https://github.com/gokulm/BoltOn) is an open source application framework to help develop simple/modular/extensible/unit-testable .NET applications (Console, MVC, WebAPI, Windows Service etc.,) with reduced boilerplate code. The components are written in such a way that they're [modular](https://en.wikipedia.org/wiki/Modular_programming), thus they can be bolted on with other components and interchanged easily, and hence the name Bolt-On. 

Installation
------------
To install BoltOn in your .NET application, type the following command in the Package Manager Console window:

    PM> Install-Package BoltOn

From CLI:

    dotnet add package BoltOn

Here is the [list of NuGet Packages](https://www.nuget.org/packages?q=BoltOn). 

Configuration
-------------
After installing the package, call BoltOn() and TightenBolts() extension methods in ConfigureServices() and Configure() methods respectively. 

    namespace BoltOn.Samples.WebApi
    {
        public class Startup
        {
            public Startup(IConfiguration configuration)
            {
                Configuration = configuration;
            }

            public IConfiguration Configuration { get; }

            public void ConfigureServices(IServiceCollection services)
            {
                services.AddMvc();
                services.BoltOn();
            }

            public void Configure(IApplicationBuilder app,
                IHostApplicationLifetime lifeTime)
            {
                app.UseMvc();
                app.ApplicationServices.TightenBolts();
                lifetime.ApplicationStopping.Register(
                    () => app.ApplicationServices.LoosenBolts());
            }
        }
    }

To use other BoltOn packages and/or add other assemblies, add them using options:

    services.BoltOn(options =>
    {
        options.BoltOnEFModule();
        options.BoltOnAssemblies(typeof(PingHandler).Assembly);
    });

** Note: ** BoltOn uses .NET core's dependency injection internally. In case if you want to use any other DI framework, you can configure it after the BoltOn() call. 

BoltOn()
--------
This method does the following:

* Registers all the interfaces in the calling assembly and assemblies passed to BoltOnAssemblies() method with **single** implementation as trasient.
* Registers all the [Requestor](requestor) handlers.

**Note:** 

* The order of the methods called is important. If BoltOnEFModule() is called after BoltOnAssemblies(), the assemblies passed to the latter will override the registrations.
* To exclude classes from registration, decorate them with `[ExcludeFromRegistration]` attribute.
* Use the BoltOnOptions' extension method like BoltOnEFModule to attach the other modules. Each and every module calls other extension methods to attach their own dependent modules. 

TightenBolts()
--------------
This extension method scans all the implementations of `IPostRegistrationTask` interface in the assemblies passed to BoltOnAssemblies() and executes them. The post registration tasks support constructor injection.

To run any task that involves resolving dependencies, like seeding data using any of the registered DbContexts, implement `IPostRegistrationTask`. 

Example:

    public class CustomPostRegistrationTask : IPostRegistrationTask
    {
        private readonly SchoolDbContext _schoolDbContext;

        public CustomPostRegistrationTask(SchoolDbContext schoolDbContext)
        {
            _schoolDbContext = schoolDbContext;
        }

        public void Run()
        {
            _schoolDbContext.Database.EnsureCreated();
            _schoolDbContext.Set<Student>().Add(new Student
            {
                Id = 1,
                FirstName = "a",
                LastName = "b"
            });
            _schoolDbContext.SaveChanges();
            _schoolDbContext.Dispose();
        }
    }

LoosenBolts()
-------------
This extension method scans all the implementations of `ICleanupTask` interface in the assemblies passed to BoltOnAssemblies() and executes them. This is mainly used to dispose and perform other clean-up tasks.

Example:

    public class CleanupTask : ICleanupTask
    {
        private readonly IServiceProvider _serviceProvider;

		public CleanupTask(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
		}

        public void Run()
        {
            var busControl = _serviceProvider.GetService<IBusControl>();
            busControl?.Stop();
        }
    }