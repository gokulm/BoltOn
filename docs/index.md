BoltOn is an [open source](https://github.com/gokulm/BoltOn) library which can be used to build any .NET application (like Console, MVC, WebAPI, Windows Service etc.,) with proper separation of concerns quickly.

Quick start
============

How to install?
-------------

There are a [couple of packages](https://www.nuget.org/packages?q=BoltOn) for BoltOn available on NuGet. To install BoltOn in your .NET application, type the following command into the Package Manager Console window:

    PM> Install-Package BoltOn

How to configure?
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
        options.AddEntityFrameworkModule();
        options.AddMediatorEntityFrameworkModule();
        options.AddAssemblies(typeof(TestHandler).Assembly);
    });

BoltOn uses .NET core's dependency injection internally. In case if you want to use any other DI framework, you can configure it after the BoltOn() call. 

What does BoltOn() do? 
----------------------
It initializes the executing assembly, all the assemblies of the other NuGet packages and the assemblies passed to BoltOnAssemblies() to a collection, sorts them based on the assembly dependencies, and finally scans for all the classes that implement **IBootstrapperRegistrationTask** and executes them. A built-in registration task called **BoltOnRegistrationTask** registers all the types that follow the convention like the interface ITestService and its implementation TestService as trasient. To exclude the types from getting registered by convention, decorate the classes with **[ExcludeFromRegistration]** attribute. For all the other registration scopes like scoped or singleton, or to register types that are not like the interface ITestService and its implementation TestService, you could implement **IBootstrapperRegistrationTask** and add them in it.
