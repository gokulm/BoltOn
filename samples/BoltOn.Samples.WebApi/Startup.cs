using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BoltOn.Samples.Domain;
using BoltOn.Data.EF;
using BoltOn.Mediator.Data.EF;

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
				options.BoltOnDataEF();
				options.BoltOnMediatorDataEF();
				options.BoltOnAssemblies(typeof(TestHandler).Assembly);
			});
		}

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseMvc();
			app.ApplicationServices.UseBoltOn();
		}
    }
}
