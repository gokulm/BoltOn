BoltOn is an [open source](https://github.com/gokulm/BoltOn) library which can be used to build a .NET application (like Console, MVC, WebAPI, Windows Service etc.,) with proper separation of concerns quickly.

Quick start
============

Installation
-------------

There are a [couple of packages](https://www.nuget.org/packages?q=BoltOn) for BoltOn available on NuGet. To install BoltOn in your **.NET application**, type the following command into the Package Manager Console window:

    PM> Install-Package BoltOn

Configuration
--------------

After installing the package, call BoltOn() and UseBoltOn() extension methods in ConfigureServices() and Configure() methods respectively. 

    .
    .
    using BoltOn;

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
                services.BoltOn(options => 
                {
                    options.BoltOnAssemblies(<collection of assemblies that should be scanned>)
                });
            }

            public void Configure(IApplicationBuilder app, IHostingEnvironment env)
            {
                app.UseMvc();
                app.ApplicationServices.UseBoltOn();
            }
        }
    }