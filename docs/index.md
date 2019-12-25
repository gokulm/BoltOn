[**BoltOn**](https://github.com/gokulm/BoltOn) is an open source application framework which can be used to build any type of .NET applications like Console, MVC, WebAPI, Windows Service etc., with [modularity](https://en.wikipedia.org/wiki/Modular_programming) being the crux of it. Though it doesn't have all the components to call it as an enterprise application framework (EAF) currently, the plan is to make it an EAF eventually. The components are written in such a way that they're modular, thus they can be bolted on with other components and interchanged easily, and hence the name Bolt-On. 

Installation
------------
There are a [couple of packages](https://www.nuget.org/packages?q=BoltOn) for BoltOn available on NuGet, out of which BoltOn package is the core. To install BoltOn in your .NET application, type the following command in the Package Manager Console window:

    PM> Install-Package BoltOn

From CLI:

    dotnet add package BoltOn

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

            public void Configure(IApplicationBuilder app, IHostingEnvironment env)
            {
                app.UseMvc();
                app.ApplicationServices.TightenBolts();
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

* It groups the executing assembly, all the assemblies of the other modules and the assemblies passed to BoltOnAssemblies() to a collection, sorts them based on the assembly dependencies, and finally scans for all the classes that implement `IRegistrationTask` and executes them in the order of the assembly dependencies. 
<br />The assemblies collection can be accessed from `RegistrationTaskContext` and `PostRegistrationTaskContext` of the registration tasks.
* A built-in registration task called [BoltOnRegistrationTask](https://github.com/gokulm/BoltOn/blob/master/src/BoltOn/Bootstrapping/BoltOnRegistrationTask.cs) registers all the interfaces with **single** implementation as trasient. 

**Custom registrations:**

* To exclude classes from registration, decorate them with `[ExcludeFromRegistration]` attribute.
* For all the other registration scopes like scoped or singleton, or to register interfaces with more than one implementations, implement `IRegistrationTask` and use the context.Container (which is of type `IServicesCollection`) to register them.

Example:

    public class CustomRegistrationTask : IRegistrationTask
	{
		public void Run(RegistrationTaskContext context)
		{
			var container = context.Container;
			container.AddSingleton<IUnitOfWorkOptionsBuilder, CustomUnitOfWorkOptionsBuilder>();
			container.AddScoped<ITestService, TestService>();
		}
	}

** Note: **
Use the BoltOnOptions' extension method like BoltOnEFModule to attach the other modules. Each and every module calls other extension methods to attach their own dependent modules. 

TightenBolts()
--------------
This extension method scans all the `IPostRegistrationTask` in the assembly collection formed by BoltOn() and executes them.

To run any task that involves resolving dependencies, like seeding data using any of the registered DbContexts, implement `IPostRegistrationTask`. 

Example:

    public class CustomPostRegistrationTask : IPostRegistrationTask
    {
        public void Run(PostRegistrationTaskContext context)
        {
            var serviceProvider = context.ServiceProvider;
            var schoolDbContext = serviceProvider.GetService<TestDbContext>();
            testDbContext.Database.EnsureCreated();
        }
    }

Cleanup
-------
BoltOn internally uses a class called [Bootstrapper](https://github.com/gokulm/BoltOn/blob/master/src/BoltOn/Bootstrapping/Bootstrapper.cs) to invoke all the registration and post registration tasks, and when it gets disposed, it calls cleanup tasks in all the modules. It's basically done by scanning all the `ICleanupTask` in the assembly collection formed by BoltOn() and executing them.

The cleanup tasks can be invoked on demand using [BoltOnAppCleaner](utilities/#boltonappcleaner).
