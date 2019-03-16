BoltOn is an [open source](https://github.com/gokulm/BoltOn) library which can be used to build any .NET application (like Console, MVC, WebAPI, Windows Service etc.,) with proper separation of concerns quickly.

Installation?
-------------

There are a [couple of packages](https://www.nuget.org/packages?q=BoltOn) for BoltOn available on NuGet. To install BoltOn in your .NET application, type the following command into the Package Manager Console window:

    PM> Install-Package BoltOn

Configuration?
--------------

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
        options.BoltOnMediatorEFModule();
        options.BoltOnAssemblies(typeof(PingHandler).Assembly);
    });

BoltOn uses .NET core's dependency injection internally. In case if you want to use any other DI framework, you can configure it after the BoltOn() call. 

How to use Mediator?
--------------------
Create a request class by implementing `IQuery<PongResponse>` (requests can be created by implementing other interfaces too)

    public class PingRequest : IQuery<PongResponse>
	{
	}

What does BoltOn() do? 
----------------------
* It initializes the executing assembly, all the assemblies of the other NuGet packages and the assemblies passed to AddAssemblies() to a collection, sorts them based on the assembly dependencies, and finally scans for all the classes that implement `IBootstrapperRegistrationTask` and executes them. 
* A built-in registration task called `BoltOnRegistrationTask` registers all the types that follow the convention like the interface ISomeService and its implementation SomeService as trasient. 
* To exclude the types from getting registered by convention, decorate the classes with `[ExcludeFromRegistration]` attribute. 
* For all the other registration scopes like scoped or singleton, or to register types that are not like the interface ISomeService and its implementation SomeService, you could implement `IBootstrapperRegistrationTask` and add them in it.

Request, Response and RequestHandler
-----------------------------------
To create a request class, implement any of these interfaces: 

* `IRequest`
<br /> To create a request that doesn't have any response and doesn't require unit of work
* `IRequest<out TResponse>` 
<br /> To create a request with response of type TResponse and doesn't require unit of work
* `ICommand`
<br /> To create a request that doesn't have any response and that requires unit of work. A transaction with isolation level ReadCommitted will be started for the requests that implement this interface. 
* `ICommand<out TResponse>` 
<br /> To create a request with response of type TResponse and that requires require unit of work. A transaction with isolation level ReadCommitted will be started for the requests that implement this interface.
* `IQuery<out TResponse>`
<br /> To create a request with response of type TResponse and that requires unit of work. A transaction with isolation level ReadCommitted will be started for the requests that implement this interface. 
<br /> If **BoltOn.Mediator.Data.EF** is installed and bolted, DbContexts' ChangeTracker.QueryTrackingBehavior will be set to `QueryTrackingBehavior.NoTracking` and ChangeTracker.AutoDetectChangesEnabled will be set to false.
* `IStaleQuery<out TResponse>` 
<br /> To create a request with response of type TResponse and that requires require unit of work. A transaction with isolation level ReadUncommitted will be started for the requests that implement this interface.

**Note:** You could modify the transaction's default isolation level and time out by implementing `IUnitOfWorkOptionsBuilder` or by inheriting `UnitOfWorkOptionsBuilder` and overriding Build method.


