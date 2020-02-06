using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BoltOn.Data.EF;
using BoltOn.Samples.Application.Handlers;
using BoltOn.Bus.MassTransit;
using BoltOn.Samples.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using MassTransit;
using System;
using Microsoft.Extensions.Hosting;

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
			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
			services.BoltOn(options =>
			{
				options.BoltOnEFModule();
				options.BoltOnMassTransitBusModule();
				options.BoltOnCqrsModule();
				options.BoltOnAssemblies(typeof(PingHandler).Assembly, typeof(SchoolWriteDbContext).Assembly);
			});
			
			var writeDbConnectionString = Configuration.GetValue<string>("SqlWriteDbConnectionString");
            var readDbConnectionString = Configuration.GetValue<string>("SqlReadDbConnectionString");
            var rabbitmqUri = Configuration.GetValue<string>("RabbitMqUri");
			var rabbitmqUsername = Configuration.GetValue<string>("RabbitMqUsername");
			var rabbitmqPassword = Configuration.GetValue<string>("RabbitMqPassword");
            services.AddMassTransit(x =>
            {
                x.AddBus(provider => MassTransit.Bus.Factory.CreateUsingRabbitMq(cfg =>
                {
                    var host = cfg.Host(new Uri(rabbitmqUri), hostConfigurator =>
                    {
                        hostConfigurator.Username(rabbitmqUsername);
                        hostConfigurator.Password(rabbitmqPassword);
                    });
                }));
            });

            services.AddDbContext<SchoolWriteDbContext>(options =>
            {
                options.UseSqlServer(writeDbConnectionString);
            });

            services.AddDbContext<SchoolReadDbContext>(options =>
            {
                options.UseSqlServer(readDbConnectionString);
            });

        }

		public void Configure(IApplicationBuilder app, IHostApplicationLifetime appLifetime)
		{
			app.UseRouting(); 
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            app.ApplicationServices.TightenBolts();
			appLifetime.ApplicationStopping.Register(() => app.ApplicationServices.LoosenBolts());
		}
	}
}
