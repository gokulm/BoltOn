BoltOn is an [open source](https://github.com/gokulm/BoltOn) library which can be used to build any .NET application (like Console, MVC, WebAPI, Windows Service etc.,) with proper separation of concerns quickly.

Installation
------------

There are a [couple of packages](https://www.nuget.org/packages?q=BoltOn) for BoltOn available on NuGet. To install BoltOn in your .NET application, type the following command into the Package Manager Console window:

    PM> Install-Package BoltOn

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
                services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
                services.BoltOn();
            }

            public void Configure(IApplicationBuilder app, IHostingEnvironment env)
            {
                app.UseMvc();
                app.ApplicationServices.TightenBolts();
            }
        }
    }

In case if you want to use other BoltOn packages and/or add other assemblies, you can add them using options:

    services.BoltOn(options =>
    {
        options.BoltOnEFModule();
        options.BoltOnAssemblies(typeof(PingHandler).Assembly);
    });

BoltOn uses .NET core's dependency injection internally. In case if you want to use any other DI framework, you can configure it after the BoltOn() call. 

BoltOn()
--------
This extension method does the following:

* It groups the executing assembly, all the assemblies of the other modules and the assemblies passed to BoltOnAssemblies() to a collection, sorts them based on the assembly dependencies, and finally scans for all the classes that implement `IBootstrapperRegistrationTask` and executes them in the order of the assembly dependencies. 
<br />The assemblies collection can be accessed from `RegistrationTaskContext` and `PostRegistrationTaskContext` of the registration tasks.
* A built-in registration task called `BoltOnRegistrationTask` registers all the interfaces with **single** implementation as trasient. 
* To exclude classes from registration, decorate them with `[ExcludeFromRegistration]` attribute.
* For all the other registration scopes like scoped or singleton, or to register interfaces with more than one implementations, implement `IBootstrapperRegistrationTask` and use the context.Container to register them.

Example:

    public class CustomRegistrationTask : IBootstrapperRegistrationTask
	{
		public void Run(RegistrationTaskContext context)
		{
			var container = context.Container;
			container.AddSingleton<IUnitOfWorkOptionsBuilder, CustomUnitOfWorkOptionsBuilder>();
			container.AddScoped<ITestService, TestService>();
		}
	}

Use the BoltOnOptions' extension method like BoltOnEFModule to attach the other modules. Each and every module calls other extension methods to attach their own dependent modules. 

TightenBolts()
--------------
This extension method does the following:

* It scans all the `IBootstrapperPostRegistrationTask` in the assemblies collection formed by BoltOn() and executes them.
* In case if you want to run any task that involves resolving dependencies, like seeding data using any of the registered DbContexts, implement `IBootstrapperPostRegistrationTask`. 

Example:

    public class CustomPostRegistrationTask : IBootstrapperPostRegistrationTask
    {
        public void Run(PostRegistrationTaskContext context)
        {
            var serviceProvider = context.ServiceProvider;
            var schoolDbContext = serviceProvider.GetService<TestDbContext>();
            testDbContext.Database.EnsureCreated();
        }
    }

Logging
-------
BoltOn uses .NET Core's logger internally, with just a custom adapter to help in unit testing. You could use any logging provider as you wish, or you could inherit `BoltOnNetStandardLoggerAdapter<TType>` and override the logging methods.

Utilities
---------
* **Check.Requires**
<br>
There are instances where you have to check for a condition and throw exception if the condition fails, in those instances you could use Check.Requires

    Example:

        Check.Requires(_serviceCollection != null, "ServiceCollection not initialized"); 

    is equivalent to

        if(_serviceCollection == null)
            throw new Exception("ServiceCollection not initialized");

    and custom exceptions can be thrown like this:

        Check.Requires<CustomException>(_serviceCollection != null, "ServiceCollection not initialized"); 

* **IBoltOnClock/BoltOnClock**
<br>
There are instances where you have to use static properties DateTime.Now or DateTimeOffset.UtcNow, which makes hard to unit test, in those instances you could inject IBoltOnClock