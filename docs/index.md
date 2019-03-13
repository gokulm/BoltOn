BoltOn is an [open source](https://github.com/gokulm/BoltOn) library which can be used to build any .NET application (like Console, MVC, WebAPI, Windows Service etc.,) with proper separation of concerns quickly.

Quick start
============

How to install?
-------------

There are a [couple of packages](https://www.nuget.org/packages?q=BoltOn) for BoltOn available on NuGet. To install BoltOn in your **.NET application**, type the following command into the Package Manager Console window:

    PM> Install-Package BoltOn

How to configure?
--------------

After installing the package, call BoltOn() and UseBoltOn() extension methods in ConfigureServices() and Configure() methods respectively. 

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
                app.ApplicationServices.UseBoltOn();
            }
        }
    }

In case if you want to use other BoltOn packages and/or add other assemblies, you can add them using options:

    services.BoltOn(options =>
    {
        options.BoltOnEntityFramework();
        options.BoltOnMediatorEntityFramework();
        options.BoltOnAssemblies(typeof(TestHandler).Assembly);
    });

BoltOn uses .NET core's dependency injection internally. In case if you want to use any other DI framework, you can configure it after the BoltOn() call. 

What does BoltOn() do? 
----------------------
It initializes the executing assembly, all the assemblies of the other NuGet packages and the assemblies passed to BoltOnAssemblies() to a collection, sorts them based on the assembly dependencies, and finally scans for all the classes that implement **IBootstrapperRegistrationTask** and executes them. A built-in registration task called **BoltOnRegistrationTask** registers all the types as trasient that follow the convention like the interface ITestService and its implementation TestService. In order to exclude the types from getting registered by convention, the types should be decorated with **[ExcludeFromRegistration]**. For all the other custom registrations like scoped or singleton registrations, you could implement **IBootstrapperRegistrationTask**.
