BoltOn is an [open source](https://github.com/gokulm/BoltOn), light-weight and modular application framework that can be used in any .NET application types like Console Application, MVC, WebAPI, Windows Service etc.

Quick start
============

Installation
-------------

There are a [couple of packages](https://www.nuget.org/packages?q=BoltOn) for BoltOn available on NuGet. To install BoltOn in your **.NET application**, type the following command into the Package Manager Console window:

    PM> Install-Package BoltOn

Configuration
--------------

After installing the package, call BoltOn() and UseBoltOn() extension methods in ConfigureServices() and Configure() methods respectively. To use UseBoltOn() extension method, install the BoltOn.AspNetCore package. 

    .
    .
    using BoltOn;
    using BoltOn.AspNetCore;

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
                app.UseBoltOn();
            }
        }
    }