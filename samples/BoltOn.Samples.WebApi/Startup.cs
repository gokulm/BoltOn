using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BoltOn.Data.EF;
using BoltOn.Samples.Application.Handlers;
using BoltOn.Data.CosmosDb;
using BoltOn.Bus.MassTransit;
using BoltOn.Utilities;
using BoltOn.Samples.Infrastructure.Data;

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
				options.BoltOnEFModule();
				options.BoltOnCosmosDbModule();
				options.BoltOnMassTransitBusModule();
				options.BoltOnCqrsModule();
				options.BoltOnAssemblies(typeof(PingHandler).Assembly, typeof(SchoolDbContext).Assembly);
			});
		}

		public void Configure(IApplicationBuilder app, IApplicationLifetime appLifetime)
		{
			app.UseMvc();
			app.ApplicationServices.TightenBolts();
			appLifetime.ApplicationStopping.Register(() => app.ApplicationServices.LoosenBolts());
		}
	}
}
